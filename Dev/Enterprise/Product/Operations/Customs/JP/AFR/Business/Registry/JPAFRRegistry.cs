using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.AFR.Business
{
	public sealed class JPAFRRegistry : RegistryItemSet
	{
		#region constructor

		public static JPAFRRegistry Instance
		{
			get { return instance ?? (instance = new JPAFRRegistry()); }
		}

		[ThreadStatic]
		static JPAFRRegistry instance;

		JPAFRRegistry() { }

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Japan_AFR => CombineCategories(Customs_Japan, ResString.GetMultilingualString("A285ADE1-019E-4252-A448-170008A32C14", "AFR"));
		}

		#endregion

		public BooleanRegistryItem AFRAddSCACtoBillsDuringSync
		{
			get
			{
				return GetItem("AFRAddSCACtoBillsDuringSync", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"AFRAddSCACtoBillsDuringSync",
						Categories.Customs_Japan_AFR,
						ResString.GetMultilingualString("214656C9-E3F6-4873-B5A8-19DB5FB7F71B", "Add Carrier Code to Master Bill during Consol synchronization or Sailing Bills importing?"),
						ResString.GetMultilingualString("7C5875EA-1016-4C4B-8B38-EF3CCF5AFCEF", "For Forwarder Manifesting\r\n\r\nIf 'Yes' it assumed that for SEA shipments, the master bill number of your consol doesn't have the four letter Carrier Code at the beginning. When synchronizing data to the AFR from Consol, in order to meet JP AFR (Advance Filing Rules) message requirement, the corresponding carrier code of designated Shipping Line in the Consol will be added automatically at the beginning of the master bill if the master bill doesn't start with carrier code. You can specify the carrier code of the Shipping Line in the corresponding (Organization -> Config -> Registration Numbers) with 'Country/Region Of Issue' set as 'JP' and 'Type' set as 'CCC'. If the master bill in the Consol is longer than 31 characters, the last 31 characters will be combined with the carrier code as the master bill number in the AFR message.\r\n\r\nFor Carrier Manifesting\r\n\r\nIf 'Yes' it assumes that the Ocean Bills for the corresponding Sailing Schedule doesn't have the four letter Carrier Code at the beginning. When importing bills from the linked sailing schedule, in order to meet JP AFR message requirement, the corresponding carrier code will be added automatically at the beginning of the Ocean Bill Number."),
						RegistryStorageFlags.Company,
						true);
					return result;
				});
			}
		}

		public StringRegistryItem AFRNVOCCIDtoAddtoBillDuringSync
		{
			get
			{
				return GetItem("AFRNVOCCIDtoAddtoBillDuringSync", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"AFRNVOCCIDtoAddtoBillDuringSync",
						Categories.Customs_Japan_AFR,
						ResString.GetMultilingualString("F85921AB-6A50-468D-A630-A620B4CA3468", "House Bill NVOCC Code"),
						ResString.GetMultilingualString("1699F66F-6F27-4E19-8E93-2648C73EDE3D", "JP AFR (Advance Filing Rules) messages require the house bill to start with NVOCC code assigned by 'NACCS Reporter ID Issuance System'. If the value is put in the below text box, it is assumed that for SEA shipments, the house bill doesn't start with the NVOCC code. When synchronizing data to AFR bill from Shipment, the value specified below will be added automatically at the beginning of the house bill to meet JP AFR message requirement if current bill number doesn't start with NVOCC code. If your house bill in the corresponding shipment is longer than 31 characters, the last 31 characters will be combined with the NVOCC code specified below to form house bill number in the AFR message."),
						new StringRegistryDataType(CharacterCase.Upper, 3, 4),
						RegistryStorageFlags.Company);
					return result;
				});
			}
		}

		public AFRReporterIDRegistryItem AFRReporterIDForDocument
		{
			get
			{
				return GetItem(nameof(AFRReporterIDForDocument), delegate
				{
					var result = new AFRReporterIDRegistryItem(
						"AFRReporterIDForDocument",
						Categories.Customs_Japan_AFR,
						ResString.GetMultilingualString("EA5FEA70-88E2-44B8-B217-F5ABD133F440", "AFR Reporter ID"),
						ResString.GetMultilingualString("9AB8A6F5-3CB7-4A8D-834E-E78731ADADA8", "The AFR Reporter ID specified in this registry Item will be used in the electronic messaging of AFR reporting and the Document printing on AFR Job. The reporter ID should be a 5 character string which you get from 'NACCS Reporter ID Issuance System'."))
					{
						OnUpdateAction = OnUpdateAction,
						OnAllValuesSavedAction = OnAllValuesSavedAction
					};
					return result;
				});
			}
		}

		public void OnAllValuesSavedAction()
		{
			SendCredentials();
			Factory.Save();
			Factory.CleanUp();
			factory = null;
			changedCompanyPks.Clear();
		}

		void SendCredentials()
		{
			var parentCredential = new CredentialSender(AFRReporterHelper.ConfigurationName);

			var systemReporterID = Instance.AFRReporterIDForDocument.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (!systemReporterID.ReporterID.IsEmpty)
			{
				var systemCredential = CredentialSender.CreateCredential(CurrentPwd, systemReporterID.ReporterID, systemReporterID.Password);
				parentCredential.AddItems(systemCredential);
			}

			var groups = GetCompanyGroups().ToArray();
			parentCredential.AddItems(groups);

			parentCredential.SendCredential(Factory);
		}

		IEnumerable<Group> GetCompanyGroups()
		{
			var companies = new GlbCompany.Loader(Factory).LoadCompanies(activeCompaniesOnly: true);

			foreach (var company in companies)
			{
				var reporter = company.GetReporter();

				if (reporter != null)
				{
					if (reporter.ReporterID.IsEmpty)
					{
						if (changedCompanyPks.Contains(company.PK))
						{
							yield return new Group() { Type = CompanyType, Reference = company.GC_Code };
						}
					}
					else
					{
						var group = new Group() { Type = CompanyType, Reference = company.GC_Code };

						var credential = CredentialSender.CreateCredential(CurrentPwd, reporter.ReporterID, reporter.Password);
						group.Items = new object[] { credential };

						yield return group;
					}
				}
			}
		}

		const string CurrentPwd = "Current";
		const string CompanyType = "Company";

		public void OnUpdateAction(Guid companyPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			var newAFRReporterIDRecord = (AFRReporterID)newValue;
			changedCompanyPks.Add(companyPK);

			var currentFallbackLevel = newAFRReporterIDRecord.CurrentFallbackLevel;
			var currentLevelType = currentFallbackLevel?.Level;
			if (currentLevelType == RegistryStorageFlags.Company || currentLevelType == RegistryStorageFlags.System)
			{
				var currentCompanyPK = currentFallbackLevel.CompanyPK(false);
				var currentReporterID = newAFRReporterIDRecord.ReporterID;

				if (!currentReporterID.IsEmpty)
				{
					var companies = new GlbCompany.Loader(Factory).LoadCompanies(activeCompaniesOnly: true);

					var otherSameReporterIdCompanies = companies
						.Where(x => x.PK != currentCompanyPK && x.GetReporter().ReporterID == currentReporterID)
						.ToArray();
					var systemIDRegistry = JPAFRRegistry.Instance.AFRReporterIDForDocument.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
					var isSystemIDTheSame = currentLevelType != RegistryStorageFlags.System && (systemIDRegistry?.ReporterID ?? ZString.Empty) == currentReporterID;

					if (otherSameReporterIdCompanies.Any() || isSystemIDTheSame)
					{
						const string nameForSystem = "System";
						var currentCompnayCode = companies.FirstOrDefault(c => c.PK == currentCompanyPK)?.GC_Name.ToString() ?? nameForSystem;
						var sameIDUsageList = new List<ZString>();
						sameIDUsageList.AddRange(otherSameReporterIdCompanies.Select(x => x.GC_Name));
						if (isSystemIDTheSame)
						{
							sameIDUsageList.Add(nameForSystem);
						}
						var otherCompanyCodes = ZString.Join(", \r\n", sameIDUsageList.ToArray());

						if (Globals.Message.Show(
								ResString.GetMultilingualString("9f397bf8-3c34-4b30-aee5-32130db26dff",
									"The AFR Reporter ID '{0}' for '{1}'is also used by:\r\n{2}.\r\n\r\nWould you like to apply the new Password to all usage?",
									currentReporterID, currentCompnayCode, otherCompanyCodes),
								ResString.GetMultilingualString("cabb1ef9-4ebc-441b-a815-01e866ee51a4", "AFR Reporter ID"),
								ZMessageBoxButtons.YesNo,
								ZMessageBoxIcon.Warning) == ZDialogResult.Yes)
						{
							foreach (var company in otherSameReporterIdCompanies)
							{
								company.SetReporter(newAFRReporterIDRecord);
							}
							if (isSystemIDTheSame)
							{
								JPAFRRegistry.Instance.AFRReporterIDForDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newAFRReporterIDRecord);
							}
						}
					}
				}
			}
		}

		readonly HashSet<ZGuid> changedCompanyPks = new HashSet<ZGuid>();

		BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory { NameForDebugging = "JPAFRRegistry" };
		BusinessObjectFactory factory;

		public CodePairRegistryItem SendMessageAcknowledgements
		{
			get
			{
				return GetItem(
					"AFRSendMessageAcknowledgements",
					() => new CodePairRegistryItem(
							"AFRSendMessageAcknowledgements",
							Categories.Customs_Japan_AFR,
							ResString.GetMultilingualString("6CEA59D4-B38B-44BD-9B19-859168BF7DFF", "Send AFR Message Acknowledgements"),
							ResString.GetMultilingualString("14EB9A2C-B1C5-4868-8057-02AD1B666B8D", "Send AFR message acknowledgements to staff member, nominated group or combination of both"),
							new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.EmailTo)),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							Enterprise.Core.Constants.EmailTo.StaffMember)
					);
			}
		}

		public GuidRegistryItem SendMessageAcknowledgementsToGroup
		{
			get
			{
				return GetItem(
					"AFRSendMessageAcknowledgementsToGroup",
					() => new GuidRegistryItem(
							"AFRSendMessageAcknowledgementsToGroup",
							Categories.Customs_Japan_AFR,
							ResString.GetMultilingualString("6E64E531-3910-4362-B494-55A2B38043D6", "Send AFR Message Acknowledgements To Group"),
							ResString.GetMultilingualString("CC261CCC-AA05-4590-8C6C-960E0A5C4A31", "Send AFR message acknowledgements to selected group"),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryConstants.GroupPKs.Notification)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					});
			}
		}

		public CodePairRegistryItem SendMessageErrors
		{
			get
			{
				return GetItem(
					"AFRSendMessageErrors",
					() => new CodePairRegistryItem(
							"AFRSendMessageErrors",
							Categories.Customs_Japan_AFR,
							ResString.GetMultilingualString("254427FD-E06E-4254-A96F-D7607E6B5F2C", "Send AFR Message Errors"),
							ResString.GetMultilingualString("CF896004-2AE2-4C31-B474-6EFD3175A20B", "Send AFR message errors to staff member, nominated group or combination of both"),
							OLookUpEditType.EmailTo,
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							Enterprise.Core.Constants.EmailTo.StaffMember)
							);
			}
		}

		public GuidRegistryItem SendMessageErrorsToGroup
		{
			get
			{
				return GetItem(
					"AFRSendMessageErrorsToGroup",
					() => new GuidRegistryItem(
							"AFRSendMessageErrorsToGroup",
							Categories.Customs_Japan_AFR,
							ResString.GetMultilingualString("AD9017B3-C583-4BFE-A7BB-CA34C42ED1C2", "Send AFR Message Errors To Group"),
							ResString.GetMultilingualString("C7822B98-752E-497E-A5E7-EA0330F6EE9C", "Send AFR message errors to selected group"),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryConstants.GroupPKs.Notification)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					});
			}
		}

		public BooleanRegistryItem IncludeAsmCldClbSubShipmentsInManifest
		{
			get
			{
				return GetItem("IncludeAsmCldClbSubShipmentsInManifest", delegate
				{
					return new BooleanRegistryItem(
						"IncludeAsmCldClbSubShipmentsInManifest",
						Categories.Customs_Japan_AFR,
						ResString.GetMultilingualString("405B28CA-E45A-44C9-BA07-FF3E345BDBA4", "Include ASM/CLD/CLB Sub Shipments in Manifest"),
						ResString.GetMultilingualString("4EA187B1-C877-4BBB-A89A-5795084CD7DC", "Include ASM/CLD/CLB Sub Shipments in Manifest"),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem IncludeBCNSubShipmentsInManifest
		{
			get
			{
				return GetItem("IncludeBCNSubShipmentsInManifest", delegate
				{
					return new BooleanRegistryItem(
						"IncludeBCNSubShipmentsInManifest",
						Categories.Customs_Japan_AFR,
						ResString.GetMultilingualString("05829415-D738-4615-BFA8-B367EC3FC5CB", "Include BCN Sub Shipments in Manifest"),
						ResString.GetMultilingualString("5AF3AC77-2B5C-4128-9CA4-EC159FB09FF7", "Include BCN Sub Shipments in Manifest"),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public DateTimeRegistryItem AFR2017EffectiveLiveDate
		{
			get
			{
				return GetItem("AFR2017EffectiveLiveDate", () => new DateTimeRegistryItem(
					"AFR2017EffectiveLiveDate",
					Categories.Customs_Japan,
					ResString.GetMultilingualString("927478C8-2C7D-4186-81FF-30CEAA2F4484", "AFR 2017 Effective Live Date"),
					ResString.GetMultilingualString("663FF84A-7E65-4DD3-AF17-B31D40BC6453", "The new function bulk Vessel Change message(CMV) will work after AFR 2017 Effective Live Date"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController,
					new DateTime(2017, 10, 7)
				));
			}
		}

		public BooleanRegistryItem AFRShowBLLFunctions =>
			GetItem("AFRShowBLLFunctions", () => new BooleanRegistryItem(
				"AFRShowBLLFunctions",
				Categories.Customs_Japan,
				ResString.GetMultilingualString("31038F1E-C922-4260-9C0F-D1CF6CCD96AE", "Show BLL Functions"),
				ResString.GetMultilingualString("AB7B6E5F-2A39-4716-AFA7-F24C3FC4E942", "BLL Functions will work only if 'Yes'"),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true)
			);
	}
}
