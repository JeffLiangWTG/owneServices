using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.Management.ShipmentProcessing
{
	class UniversalShipmentBranchLocator : RecipientBranchLocator
	{
		protected override ZGuid GetBranchPKCore(IEDIMessage message, ITopLevelDataObject topLevelDataObject, IGlbCompany glbCompany, IXmlSessionTracker logger)
		{
			var universalShipment = (UniversalShipment)topLevelDataObject;

			var companyCode = glbCompany.GC_Code;
			//1. Firstly, try to get it from Branch field
			if (universalShipment.DataContext?.CodesMappedToTarget == true && universalShipment.Branch != null)
			{
				var branchCode = universalShipment.Branch.Code.GetValueOrDefault();
				if (!string.IsNullOrEmpty(branchCode))
				{
					logger.LogVerboseOnly(LogType.Information, Res.GetString("bbacfb74-6904-4340-9c11-b3e374077bb0", "Targeting Branch '{0}', Company '{1}' – derived from target branch element ({2}->Shipment->Branch).", branchCode, companyCode, "UniversalShipment"));

					if (!glbCompany.IsBranchActive(branchCode))
					{
						logger.LogBoth(LogType.Error, Res.GetString("dff20031-c92d-474d-a94f-34960e5e04f4", "Message Rejected as Branch '{0}' is inactive.", branchCode));
						return ZGuid.Empty;
					}
					return glbCompany.GetActiveBranches().First(b => b.GB_Code.EqualsIgnoringCase(branchCode)).PK;
				}
			}
			//2. Then use addresses;
			var recipientOrganizationAddress = universalShipment.OrganizationAddressCollection.FirstOrDefault(AddressTypes.Recipient);
			var branch = GetBranchPKFromOrganizationAddress(message.Factory, recipientOrganizationAddress);

			if (branch != null)
			{
				logger.LogVerboseOnly(LogType.Information,
					Res.GetString("9aeca51e-363c-4f6b-b10e-f48700a943c3",
					"Targeting Branch '{0}', Company '{1}' – 'Recipient' {2} matches branch's Organization Proxy.", branch.GB_Code, companyCode, "OrganizationAddress"));
				return branch.PK;
			}

			//3. Then ports
			branch = GetBranchPKFromPorts(message.Factory, universalShipment, glbCompany, logger);

			if (branch != null)
			{
				return branch.PK;
			}

			return base.GetBranchPKCore(message, topLevelDataObject, glbCompany, logger);
		}

		#region Helper Methods

		IGlbBranch GetBranchPKFromPorts(BusinessObjectFactory factory, UniversalShipment universalShipment, IGlbCompany glbCompany, IXmlImportLogger logger)
		{
			var portsWithReasons = GetPortsToMatchBranch(universalShipment);

			if (factory == null || portsWithReasons == null || !portsWithReasons.Any())
			{
				return null;
			}

			var branches = glbCompany.GetActiveBranches();

			foreach (var branch in branches)
			{
				var branchCode = branch.GB_Code.ToString();

				if (string.IsNullOrEmpty(branchCode))
				{
					continue;
				}

				foreach (var portWithReason in portsWithReasons)
				{
					var port = portWithReason.Item1;
					var reason = portWithReason.Item2;

					var refUNLOCO = factory.LoadTop1<IRefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, port.Code.Value));

					if (refUNLOCO != null)
					{
						if (branch.NKUNLOCO == refUNLOCO.RL_Code)
						{
							logger.LogVerboseOnly(LogType.Information, Res.GetString("16c6a26d-0130-44d0-821e-b2f39241fd53", "Targeting Branch '{0}', Company '{1}' – {2} '{3}' matches branch's Home/Additional Port.", branchCode, glbCompany.GC_Code, reason, port.Code));
							return branch;
						}

						//load extra ports
						var queryBranchExtraPorts = new ZQuery(GlbBranchExtraPortsSchema.GY_GB, branch.PK);
						queryBranchExtraPorts.AddToFilter(GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, refUNLOCO.RL_Code);
						var branchExtraPort = factory.LoadTop1<IGlbBranchExtraPort>(queryBranchExtraPorts);

						if (branchExtraPort != null)
						{
							logger.LogVerboseOnly(LogType.Information, Res.GetString("16c6a26d-0130-44d0-821e-b2f39241fd53", "Targeting Branch '{0}', Company '{1}' – {2} '{3}' matches branch's Home/Additional Port.", branchCode, glbCompany.GC_Code, reason, port.Code));
							return branch;
						}
					}
				}
			}

			return null;
		}

		IEnumerable<Tuple<UNLOCO, string>> GetPortsToMatchBranch(UniversalShipment universalShipment)
		{
			var result = new Dictionary<string, Tuple<UNLOCO, string>>();

			Action<UNLOCO, string> addToResult = (u, s) =>
			 {
				 if (u == null)
				 {
					 return;
				 }

				 result[u.Code] = Tuple.Create(u, s);
			 };

			var recipientOrganizationAddress = universalShipment.OrganizationAddressCollection.FirstOrDefault(AddressTypes.Recipient);
			if (recipientOrganizationAddress != null)
			{
				var recipientOrganizationAddressPort = recipientOrganizationAddress.Port;
				addToResult(recipientOrganizationAddressPort, "'Recipient' OrganizationAddress Port Code");
			}

			if (universalShipment.HasRecipientRole(RecipientRoleType.SAG) || universalShipment.HasRecipientRole(RecipientRoleType.BRE))
			{
				addToResult(universalShipment.PortOfLoading, "PortOfLoading");
				addToResult(universalShipment.PortOfOrigin, "PortOfOrigin");
			}

			if (universalShipment.HasRecipientRole(RecipientRoleType.RAG) || universalShipment.HasRecipientRole(RecipientRoleType.BRI))
			{
				addToResult(universalShipment.PortOfDischarge, "PortOfDischarge");
				addToResult(universalShipment.PortOfDestination, "PortOfDestination");
			}

			if (universalShipment.HasRecipientRole(RecipientRoleType.DCF) || universalShipment.HasRecipientRole(RecipientRoleType.DCR)
				|| universalShipment.HasRecipientRole(RecipientRoleType.DCT) || universalShipment.HasRecipientRole(RecipientRoleType.DCY))
			{
				addToResult(universalShipment.PortOfLoading, "PortOfLoading");
			}

			if (universalShipment.HasRecipientRole(RecipientRoleType.ACF) || universalShipment.HasRecipientRole(RecipientRoleType.ACR)
				|| universalShipment.HasRecipientRole(RecipientRoleType.ACT) || universalShipment.HasRecipientRole(RecipientRoleType.ACY))
			{
				addToResult(universalShipment.PortOfDischarge, "PortOfDischarge");
			}

			if (universalShipment.HasRecipientRole(RecipientRoleType.CNR))
			{
				addToResult(universalShipment.PortOfOrigin, "PortOfOrigin");
			}

			if (universalShipment.HasRecipientRole(RecipientRoleType.CNE))
			{
				addToResult(universalShipment.PortOfDestination, "PortOfDestination");
			}

			return result.Values;
		}

		IGlbBranch GetBranchPKFromOrganizationAddress(BusinessObjectFactory factory, OrganizationAddress organizationAddress)
		{
			if (factory == null || organizationAddress == null)
			{
				return null;
			}

			var orgHeader = organizationAddressMatcher.GetMatchingOrgHeader(organizationAddress, factory);

			if (orgHeader != null)
			{
				var query = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, orgHeader.PK);
				query.AddToFilter(GlbBranchSchema.GB_IsActive, true);
				query.OrderBy = GlbBranchSchema.GB_Code.Name;

				var glbBranches = factory.Load<IGlbBranch>(query);

				if (glbBranches.Length > 0)
				{
					if (glbBranches.Length == 1)
					{
						return glbBranches[0];
					}

					var glbBranchesFilteredByUNLOCO = glbBranches.Where(b => b.NKUNLOCO == organizationAddress.Port.Code.ToString());

					if (glbBranchesFilteredByUNLOCO.Count() == 1)
					{
						return glbBranches.First();
					}
				}
			}

			return null;
		}

		#endregion

		readonly IOrganizationAddressMatcher organizationAddressMatcher = ObjectFactory.Get<IOrganizationAddressMatcher>();
	}
}
