using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference
{
	public class NewCashbookExchangeDiffHeader : NonPersistentBusinessObject, IObsoleteValidation
	{
		public NewCashbookExchangeDiffHeader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public NewCashbookExchangeDiffHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class Schema
		{
			public const string TableName = NewCashbookExchangeDiff.Schema.TableName;
			public const string AH_InvoiceDate = NewCashbookExchangeDiff.Schema.AH_InvoiceDate;
			public const string AH_PostDate = NewCashbookExchangeDiff.Schema.AH_PostDate;
			public const string AH_Desc = NewCashbookExchangeDiff.Schema.AH_Desc;
		}

		#region Properties

		public override SchemaGuidColumn PKSchemaColumn => AccTransactionHeaderSchema.PK;

		#region AH_InvoiceDate

		public ZDateTime AH_InvoiceDate
		{
			get => InternalNewCashbookExchangeDiff.AH_InvoiceDate;
			set
			{
				InternalNewCashbookExchangeDiff.AH_InvoiceDate = value;
				AH_InvoiceDateInfo.RefreshBinding();
				NewCashbookExchangeDiffCollection.Cast<NewCashbookExchangeDiff>().ForEach(x => x.AH_InvoiceDate = value);
			}
		}

		public ZPropertyInfo AH_InvoiceDateInfo => GetWrappedZPropertyInfo(Schema.AH_InvoiceDate, x => InternalNewCashbookExchangeDiff.AH_InvoiceDateInfo);

		#endregion

		#region AH_PostDate

		public ZDateTime AH_PostDate
		{
			get => InternalNewCashbookExchangeDiff.AH_PostDate;
			set
			{
				InternalNewCashbookExchangeDiff.AH_PostDate = value;
				AH_PostDateInfo.RefreshBinding();
				NewCashbookExchangeDiffCollection.Cast<NewCashbookExchangeDiff>().ForEach(x => x.AH_PostDate = value);
			}
		}

		public ZPropertyInfo AH_PostDateInfo => GetWrappedZPropertyInfo(Schema.AH_PostDate, x => InternalNewCashbookExchangeDiff.AH_PostDateInfo);

		#endregion

		#region AH_Desc

		public ZString AH_Desc
		{
			get => InternalNewCashbookExchangeDiff.AH_Desc;
			set
			{
				InternalNewCashbookExchangeDiff.AH_Desc = value;
				AH_DescInfo.RefreshBinding();
				NewCashbookExchangeDiffCollection.Cast<NewCashbookExchangeDiff>().ForEach(x => x.AH_Desc = value);
			}
		}

		public ZPropertyInfo AH_DescInfo => GetWrappedZPropertyInfo(Schema.AH_Desc, x => InternalNewCashbookExchangeDiff.AH_DescInfo);

		#endregion

		#region NewCashbookExchangeDiffCollection

		[ChildEditable(true)]
		public NewCashbookExchangeDiffCollection NewCashbookExchangeDiffCollection
		{
			get
			{
				if (fCashbookExchangeDiffCollection == null)
				{
					fCashbookExchangeDiffCollection = new NewCashbookExchangeDiffCollection(this);
					RegisterEditableChildObject(fCashbookExchangeDiffCollection);
				}
				return fCashbookExchangeDiffCollection;
			}
		}
		NewCashbookExchangeDiffCollection fCashbookExchangeDiffCollection;

		#endregion

		#region InternalNewCashbookExchangeDiff

		NewCashbookExchangeDiff InternalNewCashbookExchangeDiff
		{
			get
			{
				if (fNewCashbookExchangeDiff == null)
				{
					fNewCashbookExchangeDiff = Factory.CreateNewFactory().New<NewCashbookExchangeDiff>();
				}
				return fNewCashbookExchangeDiff;
			}
		}
		NewCashbookExchangeDiff fNewCashbookExchangeDiff;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateHasAtLeastOneCashbookExchangeDiff();
			base.RunPreSaveValidationCore();
		}

		void ValidateHasAtLeastOneCashbookExchangeDiff()
		{
			var errorMessage = Res.GetString("5f2cc4d7-65ed-4129-8811-26c109d60d3f", "Please select at least one bank account for bank currency adjustment.");
			RemoveRowError(errorMessage);
			if (!NewCashbookExchangeDiffCollection.Cast<NewCashbookExchangeDiff>().Any(x => x.Include))
			{
				AddRowError(errorMessage);
			}
		}

		#endregion
	}
}