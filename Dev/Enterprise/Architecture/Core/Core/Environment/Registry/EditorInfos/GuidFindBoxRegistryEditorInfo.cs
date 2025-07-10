using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration;
using WTG.StaticAnalysis.Annotation;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryFindBoxCollectionWithValidation<T> : RegistryFindBoxCollection
	{
		public RegistryFindBoxCollectionWithValidation(Action<IRegistryItem, T, Guid, Guid, Guid> validator)
		{
			Argument.NotNull(validator, nameof(validator));
			Validate = validator;
		}

		public Action<IRegistryItem, T, Guid, Guid, Guid> Validate { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "The class below is inherited in multiple places, so this class cannot be static. Therefore adding the below suppress message related to static type.")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")] //still throws even with protected constructor below
	[Immutable]
	public class RegistryFindBoxCollection
	{
		protected RegistryFindBoxCollection()
		{
		}

		public static readonly RegistryFindBoxCollection None = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection OrgHeader = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection Debtor = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection StmPrintQueue = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection ARInvoiceMenuItem = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection RefServiceLevel = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection RefCommodityCode = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection AccBankAccount = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection AccGLHeader = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection AccChargeCode = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection GlbGroup = new RegistryFindBoxCollectionWithValidation<Guid>(GroupValidation);
		public static readonly RegistryFindBoxCollection ShippingProvider = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection Broker = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection GlbDepartment = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection GlbBranch = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection GlbBranchNotCurrentCompanyRelated = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection FumigationContractors = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection GlbStaff = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection AccountDescriptors = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection OrgDebtorGroup = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection OrgCreditorGroup = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection RefCurrency = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection WhsWarehouse = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection OrgContact = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection AccTaxRate = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection GlbCompany = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection PackingDocument = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection AccInvMsg = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection ZACustomsOffice = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection RefDocType = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection TagMagnitude = new RegistryFindBoxCollection();
		public static readonly RegistryFindBoxCollection AccAlternateChart = new RegistryFindBoxCollection();

		static void GroupValidation(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (registryItem != null && EnvProxy.IsHostedWithCargowise && !EnvProxy.Instance.CurrentUser.IsSupportUser)
			{
				var glbGroup = new BusinessObjectFactory().Load<IGlbGroup>(proposedValue);
				if (glbGroup != null && glbGroup.GG_Code == "SUP")
				{
					throw new RegistryValidationException(Res.GetString("11F87A38-503B-499A-BD2A-1E616D955E3C", "Only {0} are permitted to select the SUP group.", Constants.ProductSupportName));
				}
			}
		}
	}

	/// <summary>
	/// This is only a temp hack to make the Registry work with ZGuidFindBox.
	/// Should be removed when the whole Registry is converted for Z
	/// </summary>
	public enum RegistryFindBoxFilter
	{
		None,
		BSHAndControl,
		BSHAndNonControl,
		PandL,
		PandLOrBSH,
		HDR,
		TTL,
		DisbursementChargeCode,
		CustomDeferredChargeCode,
		MrgDsbOrMjaChargeCode,
		ChineseSimplifiedAccountDescriptor,
		ChineseTraditionalAccountDescriptor,
		VietnameseAccountDescriptor,
		NonMiscDepartment,
		FreightChargeCode,
		RevenueChargeCode,
		PandLOrBSHAndNonControl_AllowDirectPost,
		BSHAndNonControl_AllowDirectPost,
		AccTaxRateTypeRated,
		AccTaxRateTypeReverseRated,
		AccTaxRateTypeExempt,
		AccTaxRateTypeCapitalRated,
		AccTaxRateTypeNotReportable,
		BSH,
		RefDocType,
		RefDocTypeForCommunicationParsedEmail,
		NonJobRelatedChargeCode,
		PandLOrBSHandNonControl,
		AUCustomsQuarantineChargeCode,
		RevenueOrNonJobRelatedChargeCode,
		GlobalDSBOrMRGChargeCode,
		VATTaxSystem,
		BSHAndNonControlDisallowDirectPost,
		PandLOrBSHAndNonControl_DisallowDirectPost,
	}

	public interface IGuidFindBoxRegistryEditorInfo : IRegistryEditorInfo
	{
		IMultilingualString CustomErrorMessage { get; }
	}

	public class GuidFindBoxRegistryEditorInfo : GuidRegistryEditorInfo, IGuidFindBoxRegistryEditorInfo
	{
		public GuidFindBoxRegistryEditorInfo(bool allowEditorNew, bool allowEditorDelete)
		{
			AllowEditorNew = allowEditorNew;
			AllowEditorDelete = allowEditorDelete;
		}

		public GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection findBoxCollection)
			: this(findBoxCollection, default)
		{
		}

		public GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection findBoxCollection, RegistryFindBoxFilter filter)
			: this(findBoxCollection, filter, null)
		{
		}

		public GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection findBoxCollection, RegistryFindBoxFilter filter, IMultilingualString customErrorMessage)
		{
			FindBoxCollection = findBoxCollection;
			FindBoxFilter = filter;
			CustomErrorMessage = customErrorMessage;
		}

		public RegistryFindBoxCollection FindBoxCollection
		{
			get { return fFindBoxCollection; }
			set { fFindBoxCollection = value; }
		}
		RegistryFindBoxCollection fFindBoxCollection = RegistryFindBoxCollection.None;

		public RegistryFindBoxFilter FindBoxFilter
		{
			get { return fFindBoxFilter; }
			set { fFindBoxFilter = value; }
		}
		RegistryFindBoxFilter fFindBoxFilter;

		public bool AllowEditorNew
		{
			get { return fAllowEditorNew; }
			set { fAllowEditorNew = value; }
		}
		bool fAllowEditorNew = true;

		public bool AllowEditorDelete
		{
			get { return fAllowEditorDelete; }
			set { fAllowEditorDelete = value; }
		}
		bool fAllowEditorDelete = true;

		bool fIsPrimaryKeyFromCodeRequired;

		public IMultilingualString CustomErrorMessage { get; }

		public bool IsPrimaryKeyFromCodeRequired
		{
			get { return fIsPrimaryKeyFromCodeRequired; }
			set { fIsPrimaryKeyFromCodeRequired = value; }
		}
	}
}
