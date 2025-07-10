using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class InvoicePostingExRateOption : RegistryBusinessObjectTemplate
	{
		public InvoicePostingExRateOption()
			: base()
		{ }

		public InvoicePostingExRateOption(string invoiceCurrencyType, string exRateOption, int offSet)
			: base()
		{
			InvoiceCurrencyType = invoiceCurrencyType;
			ExRateOption = exRateOption;
			OffSet = offSet;
		}

		public InvoicePostingExRateOptionCollection ParentCollection =>
			GetParentCollection(this, typeof(InvoicePostingExRateOptionCollection)) as InvoicePostingExRateOptionCollection
			 ?? new InvoicePostingExRateOptionCollection(CurrentFallbackLevel, Factory);

		#region Schema

		public abstract class Schema
		{
			public const string InvoiceCurrencyType = nameof(InvoiceCurrencyType);
			public const string ExRateOption = nameof(ExRateOption);
			public const string OffSet = nameof(OffSet);
		}

		#endregion

		#region Properties

		[MaxLength(3)]
		[List(nameof(ExRateOptionList))]
		[ResourceStringData("f2d1e608-f419-4882-b591-79536d2b5c1e", Caption = "Exchange Rate Option")]
		public ZString ExRateOption
		{
			get => exRateOption;
			set
			{
				CheckMaximumLength(ExRateOptionInfo, value);
				SetNonPersistentPropertyValue(ExRateOptionInfo, ref exRateOption, value);

				if (value == AccountingConstants.InvoicePostingExchangeRateOption.Default.Code)
				{
					OffSet = 0;
				}

				if (!IsValidationSuspended)
				{
					ValidateExRateOption();
				}
			}
		}
		ZString exRateOption;

		public ZPropertyInfo ExRateOptionInfo => GetZPropertyInfo(Schema.ExRateOption);

		[MaxLength(3)]
		[List(nameof(InvoiceCurrencyTypeList))]
		[ResourceStringData("4e3c58e9-ac83-46ba-9132-a9b40f42d64a", Caption = "Invoice Currency Type")]
		[ReadOnly(true)]
		public ZString InvoiceCurrencyType
		{
			get => invoiceCurrencyType;
			set
			{
				CheckMaximumLength(InvoiceCurrencyTypeInfo, value);
				SetNonPersistentPropertyValue(InvoiceCurrencyTypeInfo, ref invoiceCurrencyType, value);
				if (!IsValidationSuspended)
				{
					ValidateInvoiceCurrencyType();
				}
			}
		}
		ZString invoiceCurrencyType;

		public ZPropertyInfo InvoiceCurrencyTypeInfo => GetZPropertyInfo(Schema.InvoiceCurrencyType);

		[ResourceStringData("918680bb-fd76-4461-99a5-ea5ace59145a", Caption = "Offset")]
		[ReadOnlyMember(nameof(IsOffSetReadOnly))]
		public ZInt OffSet
		{
			get => offSet;
			set => SetNonPersistentPropertyValue(OffSetInfo, ref offSet, value);
		}
		ZInt offSet;

		public ZPropertyInfo OffSetInfo => GetZPropertyInfo(Schema.OffSet);

		bool IsOffSetReadOnly => ExRateOption == AccountingConstants.InvoicePostingExchangeRateOption.Default.Code;

		#endregion

		#region Lookups

		public CodeDescriptionPairList InvoiceCurrencyTypeList => JobConfigurationLookupsExtensions.GetInvoiceCurrencyTypeList();

		public CodeDescriptionPairList ExRateOptionList => exRateOptionList ?? (exRateOptionList = AccountingConstants.InvoicePostingExchangeRateOption.CodeList);
		CodeDescriptionPairList exRateOptionList;

		#endregion

		#region Validation

		void ValidateExRateOption()
		{
			ExRateOptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ExRateOptionInfo);
			ListValidation.ErrorIfInvalidCode(ExRateOptionInfo);
		}

		void ValidateInvoiceCurrencyType()
		{
			InvoiceCurrencyTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(InvoiceCurrencyTypeInfo);
			ListValidation.ErrorIfInvalidCode(InvoiceCurrencyTypeInfo);
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new InvoicePostingExRateOption();

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.InvoiceCurrencyType, InvoiceCurrencyType);
			writer.WriteElementString(Schema.ExRateOption, ExRateOption);
			writer.WriteElementString(Schema.OffSet, OffSet.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			InvoiceCurrencyType = reader.ReadElementString(Schema.InvoiceCurrencyType);
			ExRateOption = reader.ReadElementString(Schema.ExRateOption);
			OffSet = reader.ReadElementStringAsZInt(Schema.OffSet);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateInvoiceCurrencyType();
			ValidateExRateOption();
		}

		#endregion
	}
}
