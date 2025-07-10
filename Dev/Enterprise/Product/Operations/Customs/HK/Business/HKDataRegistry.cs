using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.HK;

namespace Enterprise.Customs.HK.Business
{
	public sealed class HKDataRegistry : RegistryItemSet, IHKCustomsDataRegistry
	{
		HKDataRegistry()
		{
		}

		public static HKDataRegistry Instance => instance ?? (instance = new HKDataRegistry());

		[ThreadStatic]
		static HKDataRegistry instance;

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_HongKong => CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("e97c0e79-2256-4bcb-8aa5-80b15200a5a0", "Hong Kong"));
			public static MultilingualString Customs_HongKong_FTPSettings => CombineCategories(Customs_HongKong, ResString.GetMultilingualString("c1af7cd6-9e91-4b80-8277-4eb4561919e1", "FTP Settings"));
		}

		#endregion

		#region Hong Kong

		public StringRegistryItem ISACFTPServerOutputAddress
		{
			get
			{
				return GetItem("ISACFTPServerOutputAddress", () =>
				{
					var result = new StringRegistryItem(
						"ISACFTPServerOutputAddress",
						Categories.Customs_HongKong_FTPSettings,
						(NoResString)"FTP Server Output Address",
						(NoResString)"The FTP server output address for ISAC. It should be entered in the following format: ftp://{SERVER_NAME}/{OUTPUT_DIRECTORY}",
						RegistryStorageFlags.Company,
						RegistryOptions.Default
					)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong,
						OnUpdateAction = ISACFTPServerOutputAddress_OnUpdateAction,
						OnAllValuesSavedAction = OnAllValuesSavedAction
					};
					return result;
				});
			}
		}

		public void ISACFTPServerOutputAddress_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			var serverOutputAddress = (string)newValue;
			var serverNamePort = GetServerNamePort(serverOutputAddress);

			SendCredential(companyPk, serverNamePort: serverNamePort);
		}

		static (string ServerName, string Folder, int Port) GetServerNamePort(string serverInputAddress)
		{
			var port = 21;
			var result = (string.Empty, string.Empty, port);

			const string pattern = @"^(?:(?'Scheme'((?i)\w+)):\/\/)?(?'ServerName'[a-zA-Z0-9_\-.]+)(?:\:(?'Port'[0-9]+))?(?'Folder'\/[^\?\s]*)?(?:\?(?'Query'\S+))?";
			var match = Regex.Match(serverInputAddress, pattern);

			if (match.Success)
			{
				var groups = match.Groups;

				string GetValueIfSuccess(string groupName)
				{
					var group = groups[groupName];
					return group != null && group.Success ? group.Value : null;
				}

				var scheme = GetValueIfSuccess("Scheme");
				if (!string.IsNullOrWhiteSpace(scheme))
				{
					var serverName = GetValueIfSuccess("ServerName");
					var folder = GetValueIfSuccess("Folder");

					port = int.TryParse(GetValueIfSuccess("Port"), out port) ? port : 21;
					result = (serverName, folder, port);
				}
			}

			return result;
		}

		public StringRegistryItem ISACFTPPassword
		{
			get
			{
				return GetItem("ISACFTPPassword", () =>
				{
					var result = new StringRegistryItem(
						"ISACFTPPassword",
						Categories.Customs_HongKong_FTPSettings,
						(NoResString)"FTP Password",
						(NoResString)"The FTP password for ISAC server",
						RegistryStorageFlags.Company,
						RegistryOptions.Default
					)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
						CountryFilterPKs = CountryFilterPKs.HongKong,
						OnUpdateAction = ISACFTPPassword_OnUpdateAction,
						OnAllValuesSavedAction = OnAllValuesSavedAction
					};
					return result;
				});
			}
		}

		public void ISACFTPPassword_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			var password = (string)newValue;

			SendCredential(companyPk, password: password);
		}

		public StringRegistryItem ISACFTPUserName
		{
			get
			{
				return GetItem("ISACFTPUserName", () =>
				{
					var result = new StringRegistryItem(
						"ISACFTPUserName",
						Categories.Customs_HongKong_FTPSettings,
						ResString.GetMultilingualString("E888C08B-A703-4F9F-97A9-047F6E1C9D37", "FTP User Name"),
						ResString.GetMultilingualString("1B0DA2DE-C334-42F1-B238-D4D25E45D29E", "The FTP user name for ISAC server"),
						RegistryStorageFlags.Company,
						RegistryOptions.Default
					)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong,
						OnUpdateAction = ISACFTPUserName_OnUpdateAction,
						OnAllValuesSavedAction = OnAllValuesSavedAction
					};
					return result;
				});
			}
		}

		public BooleanRegistryItem ISACFTPPassiveMode
		{
			get
			{
				return GetItem("ISACFTPPassiveMode", () =>
				{
					var result = new BooleanRegistryItem(
						"ISACFTPPassiveMode",
						Categories.Customs_HongKong_FTPSettings,
						ResString.GetMultilingualString("3E6491F9-7325-415F-A5B6-F91FF71CFBA3", "FTP Passive Mode"),
						ResString.GetMultilingualString("476FC322-B5FE-46B5-AAED-2BADBF9A5B98", "The FTP Passive Mode for ISAC server. Set to Yes to cause the FTP message process to select passive mode when communicating with GLS (ISAC Traxon)."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						true
					)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong,
					};
					return result;
				});
			}
		}

		public void ISACFTPUserName_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			var userName = (string)newValue;

			SendCredential(companyPk, userName: userName);
		}

		public StringRegistryItem CosacAgentCode
		{
			get
			{
				return GetItem(
					"CosacAgentCode",
					() => new StringRegistryItem(
									"CosacAgentCode",
									Categories.Customs_HongKong,
									(NoResString)"COSAC Agent Code",
									(NoResString)"The agent party id used for ISAC",
									RegistryStorageFlags.Company,
									RegistryOptions.PreserveTestValue)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong
					}
					);
			}
		}

		public StringRegistryItem HKTraxonSenderID
		{
			get
			{
				return GetItem(
					"Traxon Sender ID",
					() => new StringRegistryItem(
									"Traxon Sender ID",
									Categories.Customs_HongKong,
									(NoResString)"ISAC Sender ID",
									null,
									RegistryStorageFlags.Company,
									RegistryOptions.PreserveTestValue)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong,
						OnUpdateAction = HKTraxonSenderID_OnUpdateAction,
						OnAllValuesSavedAction = OnAllValuesSavedAction
					}
					);
			}
		}

		GlbCompanyCollection Companies => companies ?? (companies = new GlbCompanyCollection(Factory));
		GlbCompanyCollection companies;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory { NameForDebugging = "HKCustomsRegistry" });
		BusinessObjectFactory factory;

		const string ConfigurationName = "GLSHKConfiguration";
		const string CompanyType = "Company";
		const string Pima = "PIMA";

		void OnAllValuesSavedAction()
		{
			Factory.Save();
		}

		public void HKTraxonSenderID_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			var senderId = (string)newValue;

			SendCredential(companyPk, senderId);
		}

		void SendCredential(Guid companyPK, string senderId = null, (string ServerName, string Folder, int Port)? serverNamePort = null, string userName = null, string password = null)
		{
			var company = (GlbCompany)Companies.FindByPK(companyPK);
			var credentialSender = new CredentialSender(ConfigurationName);
			serverNamePort = serverNamePort ?? GetServerNamePort(Instance.ISACFTPServerOutputAddress.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));
			var ftp = CredentialSender.CreateFTP(serverNamePort.Value.ServerName
				, serverNamePort.Value.Port
				, userName ?? Instance.ISACFTPUserName.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
				, password ?? Instance.ISACFTPPassword.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty));

			var folder = serverNamePort.Value.Folder;
			if (!string.IsNullOrWhiteSpace(folder))
			{
				ftp.ReceiveFolder = folder;
			}

			var pima = CredentialSender.CreateGroup(Pima
				, senderId ?? Instance.HKTraxonSenderID.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
				, ZString.Empty);
			pima.Items = new object[] { ftp };

			var companyGroup = CredentialSender.CreateGroup(CompanyType, company.GC_Code, ZString.Empty);
			companyGroup.Items = new object[] { pima };

			credentialSender.AddItems(companyGroup);

			credentialSender.SendCredential(Factory);
		}

		public StringRegistryItem HKTraxonRecipientReferencePassword
		{
			get
			{
				return GetItem(
					"Traxon Recipient Reference",
					() => new StringRegistryItem(
									"Traxon Recipient Reference",
									Categories.Customs_HongKong,
									(NoResString)"ISAC Recipient Reference Password",
									null,
									RegistryStorageFlags.Company,
									RegistryOptions.PreserveTestValue)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong
					});
			}
		}

		public StringRegistryItem HKTraxonOutputDirectory
		{
			get
			{
				return GetItem(
					"Traxon Output Directory",
					() => new StringRegistryItem(
									"Traxon Output Directory",
									Categories.Customs_HongKong,
									(NoResString)"ISAC Output Directory",
									null,
									new DirectoryRegistryDataType(),
									null,
									RegistryStorageFlags.Company,
									RegistryOptions.PreserveTestValue, "")
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser),
						CountryFilterPKs = CountryFilterPKs.HongKong
					}
				);
			}
		}

		public GuidRegistryItem GroupToCopyTraxonResponseEmailsTo
		{
			get
			{
				return GetItem(
					"Group To Copy Traxon Response Emails To",
					() =>
					new GuidRegistryItem("Group To Copy Traxon Response Emails To",
						Categories.Customs_HongKong, (NoResString)"Group To Copy ISAC Response Emails To", null,
						RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong,
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					}
					);
			}
		}

		#region Send Other Customs Information

		public GuidArrayRegistryItem SendOtherCustomsInformation
		{
			get
			{
				return GetItem("SendOtherCustomsInformation",
					() => new GuidArrayRegistryItem("SendOtherCustomsInformation",
						Categories.Customs_HongKong,
						ResString.GetMultilingualString("DE4F0083-A24D-4C25-AA71-B1A0E386AAE5", "Send Other Customs Information"),
						ResString.GetMultilingualString("AD2D33A2-78AB-44D6-821C-94926B8572D8", @"When ""Send Other Customs Information"" is true. The system will send selected additional information in the ISAC message for exports including Consignee/Shipper/Also Notify Contact Details and Trader Identification Number providing the country of discharge of the Consol is included in this list of countries. If you include HK in this list, the additional information will be sent for imports also. The decision to send is based solely on the countries in this list matching the port of discharge on a Consol."),
						RegistryStorageFlags.Company)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong,
						EditorInfo = new CountryListRegistryEditorInfo()
					});
			}
		}

		public BooleanRegistryItem SendInfoFromShipments
		{
			get
			{
				return GetItem("SendInfoFromShipments", () =>
				{
					var result = new BooleanRegistryItem(
						"SendInfoFromShipments",
						Categories.Customs_HongKong,
						ResString.GetMultilingualString("AE50CD72-F671-44E0-9B05-B727EEF49F3D", "Send Other Customs Information - Send from Shipments"),
						ResString.GetMultilingualString("3B548F5A-7CF3-4ACE-8C07-3669701ED982", "When choosing to send additional information in the ISAC message, (refer Send Other Customs Information registry), this registry will send Group 15 information from respective shipments on the Consol, rather than from the details entered on the AWB."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false
					)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong,
					};
					return result;
				});
			}
		}

		public BooleanRegistryItem SendHsCode
		{
			get
			{
				return GetItem("SendHsCode", () =>
				{
					var result = new BooleanRegistryItem(
						"SendHsCode",
						Categories.Customs_HongKong,
						ResString.GetMultilingualString("4E87CF2D-D1C6-49BE-9BE4-F9ADE083EEDC", "Send HS Code"),
						ResString.GetMultilingualString("A4720AA4-2EDA-468B-9948-786B8CACD4FA", "If set to \"Yes\" the system will generate the HS Commodity Code segments in the ISAC Message."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						false
					)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong,
					};
					return result;
				});
			}
		}

		public BooleanRegistryItem TruncateOtherCustomsInformation
		{
			get
			{
				return GetItem("TruncateOtherCustomsInformation", () =>
				{
					var result = new BooleanRegistryItem(
						"TruncateOtherCustomsInformation",
						Categories.Customs_HongKong,
						ResString.GetMultilingualString("17520A5F-63A5-448F-AA39-BA7B1120D53F", "Truncate Supplementary Customs Information"),
						ResString.GetMultilingualString("46CD4AC3-1DEC-4E7D-A2AE-F50C988DB910", "If set to \"Yes\" the system will truncate the Supplementary Customs Information at 35 characters."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						true
					)
					{
						CountryFilterPKs = CountryFilterPKs.HongKong,
					};
					return result;
				});
			}
		}

		#endregion

		#region IHKCustomsDataRegistry

		IRegistryItem IHKCustomsDataRegistry.HKTraxonSenderID => HKTraxonSenderID;

		#endregion

		#endregion
	}
}
