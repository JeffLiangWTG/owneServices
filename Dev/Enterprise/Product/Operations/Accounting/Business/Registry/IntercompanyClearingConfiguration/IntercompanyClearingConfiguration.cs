using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class IntercompanyClearingConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string Company = "Company";
			public const string ClearingGLAccount = "ClearingGLAccount";
		}

		#endregion

		public IntercompanyClearingConfiguration()
		{
		}

		public IntercompanyClearingConfiguration(ZString companyCode)
		{
			Company = companyCode;
			ClearingGLAccount = ZGuid.Empty;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IntercompanyClearingConfiguration();
		}

		#region Bound Properties

		#region Company

		[MaxLength(3)]
		[ReadOnly(true)]
		public ZString Company
		{
			get { return company; }
			set
			{
				CheckMaximumLength(CompanyInfo, value);
				SetNonPersistentPropertyValue(CompanyInfo, ref company, value);
			}
		}

		public ZPropertyInfo CompanyInfo
		{
			get { return GetZPropertyInfo(Schema.Company); }
		}

		ZString company;
		GlbCompany CompanyBizO
		{
			get { return RegistryFactory.Instance.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, Company); }
		}

		#endregion

		#region ClearingGLAccount

		[List("GLAccounts")]
		public ZGuid ClearingGLAccount
		{
			get { return fClearingGLAccount; }
			set
			{
				SetNonPersistentPropertyValue(ClearingGLAccountInfo, ref fClearingGLAccount, value);
				if (!IsValidationSuspended)
				{
					ValidateClearingGLAccount();
				}
			}
		}
		ZGuid fClearingGLAccount;

		public ZPropertyInfo ClearingGLAccountInfo
		{
			get { return GetZPropertyInfo(Schema.ClearingGLAccount); }
		}

		public bool ClearingGLAccount_ReadOnly
		{
			get
			{
				return CompanyBizO != null && CurrentFallbackLevel != null && CurrentFallbackLevel.CompanyPK(false) == CompanyBizO.PK;
			}
		}

		public void ValidateClearingGLAccount()
		{
			ClearingGLAccountInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(ClearingGLAccountInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateClearingGLAccount();
		}

		#endregion

		#endregion

		#region Lookups

		#region GLAccounts

		public AccGLHeaderCollection GLAccounts
		{
			get
			{
				ZQuery balanceSheetQuery = new ZQuery(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
				balanceSheetQuery.AddToFilter(AccGLHeaderSchema.AG_IsActive, true);
				balanceSheetQuery.AddToFilter(AccGLHeaderSchema.AG_IsGlobal, true);
				AccGLHeaderCollection headers = new AccGLHeaderCollection(RegistryFactory.Instance, balanceSheetQuery);
				headers.Load();
				return headers;
			}
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Company, Company);
			writer.WriteElementString(Schema.ClearingGLAccount, ClearingGLAccount.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Company = reader.ReadElementString(Schema.Company);
			ClearingGLAccount = new ZGuid(reader.ReadElementString(Schema.ClearingGLAccount));
		}

		#endregion
	}
}