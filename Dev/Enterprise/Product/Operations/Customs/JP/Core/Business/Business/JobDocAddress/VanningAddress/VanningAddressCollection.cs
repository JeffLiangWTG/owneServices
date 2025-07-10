using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public class VanningAddressCollection : DependentBusinessObjectCollection<VanningAddress, CusEntryInstruction>
	{
		public VanningAddressCollection(CusEntryInstruction master) : base(master, new ZQuery(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.VanningLocationAddress))
		{
			MaxCountValidationEnable(MaxRowCount);
		}

		public const int MaxRowCount = 5;

		protected override bool AllowNewCore => base.AllowNewCore && Count < MaxRowCount;

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(VanningAddress);

		protected override SchemaGuidColumn FKSchemaColumnInDependent => JobDocAddressSchema.E2_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var vanningAddress = (VanningAddress)child;
			vanningAddress.E2_ParentTableCode = Master.TablePrefix;
			vanningAddress.E2_AddressType = DocAddressTypes.Codes.VanningLocationAddress;
			vanningAddress.DocAddressType = DocAddressType.VanningLocationAddress;

			var req = new JobDocAddressRequirement(DocAddressType.VanningLocationAddress);
			req.GetRegistrationNumberResult = GetRegistrationNumberResult;
			vanningAddress.OverrideRequirement = req;
		}

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			var child = base.CreateBusinessObjectFromRow(row);
			AddRowToDataTableIfDetached(child);
			return child;
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);

			var vanningAddress = (VanningAddress)bizO;
			vanningAddress.Instruction?.VanningLocationsSeqGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(vanningAddress);
		}

		RegistrationNumberResult GetRegistrationNumberResult(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true, delegate
			{
				if (docAddress.Organisation != null && docAddress.Address != null)
				{
					var code = docAddress.Organisation.CustomsCodes.GetOrgCusCodeForPremiseAddress(
						OrgCusCode.CodeTypes.ControlledPremisesID,
						Core.Constants.CountryCodes.Japan, docAddress.Address.PK);

					if (code != null)
					{
						return new RegistrationNumber { Number = code.OK_CustomsRegNo, NumberType = code.OK_CodeType };
					}

					code = docAddress.Organisation.CustomsCodes.GetOrgCusCodeForPremiseAddress(
						OrgCusCode.JapanCodeTypes.LPC,
						Core.Constants.CountryCodes.Japan, docAddress.Address.PK);

					if (code != null)
					{
						return new RegistrationNumber { Number = code.OK_CustomsRegNo, NumberType = code.OK_CodeType };
					}

					code = docAddress.Organisation.CustomsCodes.GetOrgCusCodeForPremiseAddress(
						OrgCusCode.JapanCodeTypes.CIE,
						Core.Constants.CountryCodes.Japan, docAddress.Address.PK);

					if (code != null)
					{
						return new RegistrationNumber { Number = code.OK_CustomsRegNo, NumberType = code.OK_CodeType };
					}
				}

				return new RegistrationNumber();
			});
		}
	}
}
