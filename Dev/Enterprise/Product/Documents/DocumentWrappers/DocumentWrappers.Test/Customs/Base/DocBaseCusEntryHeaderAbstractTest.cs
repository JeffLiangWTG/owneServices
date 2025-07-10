using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestsSubclassesOf(typeof(DocBaseCusEntryHeader))]
	public abstract class DocBaseCusEntryHeaderAbstractTest<T, TWrapper> : DocumentWrapperTestCase
			where T : CusEntryHeader
			where TWrapper : DocBaseCusEntryHeader
	{
		public void TestToString()
		{
			EntryHeaderInternal.EntryNumber = "EntryNumber";
			AssertEquals("ToString()", EntryHeaderInternal.EntryNumber, EntryHeaderWrapperInternal.ToString());
		}

		#region Abstract

		protected abstract TWrapper CreateEntryHeaderWrapper(T entryHeader);

		#endregion

		#region ZDecimal Fields

		public void TestCustomsValue()
		{
			AssertEquals("CustomsValue", EntryHeaderInternal.CustomsValue, EntryHeaderWrapperInternal.CustomsValue);
		}

		public void TestTotalAmountPayable()
		{
			AssertEquals("TotalAmountPayable", EntryHeaderInternal.TotalAmountPayable, EntryHeaderWrapperInternal.TotalAmountPayable);
		}

		public void TestGSTAmount()
		{
			AssertEquals("GSTAmount", EntryHeaderInternal.GSTAmount, EntryHeaderWrapperInternal.GSTAmount);
		}

		public void TestTotalPaid()
		{
			EntryHeaderInternal.CH_TotalPaid = 12.23M;
			AssertEquals("TotalPaid", EntryHeaderInternal.CH_TotalPaid, EntryHeaderWrapperInternal.TotalPaid);
		}

		public void TestWeight()
		{
			AssertEquals("Weight", EntryHeaderInternal.GrossWeight.Amount, EntryHeaderWrapperInternal.Weight);
		}

		public void TestCustomsValueRounded()
		{
			AssertEquals("CustomsValue", ZArchitecture.Core.Utilities.Round(EntryHeaderInternal.CustomsValue, 2), EntryHeaderWrapperInternal.CustomsValue);
		}

		public void TestTotalAmountPayableRounded()
		{
			AssertEquals("TotalAmountPayable", ZArchitecture.Core.Utilities.Round(EntryHeaderInternal.TotalAmountPayable, 2), EntryHeaderWrapperInternal.TotalAmountPayable);
		}

		public void TestCIFLocalAmount()
		{
			AssertEquals("TotalCIFAmount", EntryHeaderInternal.CIFInLocalCurrency.Amount, EntryHeaderWrapperInternal.CIFInLocalCurrency.Amount);
		}
		#endregion

		#region ZString Fields

		public void TestWeightUQ()
		{
			AssertEquals("WeightUQ", EntryHeaderInternal.GrossWeight.Unit, EntryHeaderWrapperInternal.WeightUQ);
		}

		public void TestEntryNumber()
		{
			EntryHeaderInternal.EntryNumber = "EntryNumber";
			AssertEquals("EntryNumber", EntryHeaderInternal.EntryNumber, EntryHeaderWrapperInternal.EntryNumber);
		}

		public void TestBGMReference()
		{
			EntryHeaderInternal.CH_BGMReference = "BGMReference";
			AssertEquals("BGMReference", EntryHeaderInternal.CH_BGMReference, EntryHeaderWrapperInternal.BGMReference);
		}

		public void TestStatus()
		{
			EntryHeaderInternal.CH_Status = "SSS";
			AssertEquals("Status", EntryHeaderInternal.CH_Status, EntryHeaderWrapperInternal.Status);
		}

		#endregion

		#region ZInt Fields

		public virtual void TestPackages()
		{
			AssertEquals("Packages", EntryHeaderInternal.PackagesCount, EntryHeaderWrapperInternal.Packages);
		}

		#endregion
		#region Static New

		public void TestStaticNewReturnsCountrySpecificEntryHeader()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			DocBaseCusEntryHeader docEntryHeader = DocBaseCusEntryHeader.New(entryHeader, Factory);
			AssertEquals(typeof(AU.DocCusEntryHeader), docEntryHeader.GetType());
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				EntryHeaderWrapperInternal
			};
		}

		#region Implementation

		protected T EntryHeaderInternal;
		protected TWrapper EntryHeaderWrapperInternal
		{
			get { return CreateEntryHeaderWrapper(EntryHeaderInternal); }
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return CreateEntryHeaderWrapper(EntryHeaderInternal);
		}

		protected virtual BaseJobDeclaration BaseDeclaration
		{
			get { return baseDeclaration ?? (baseDeclaration = Factory.New<BaseJobDeclaration>()); }
		}
		BaseJobDeclaration baseDeclaration;

		protected virtual ZString ImportJobMessage
		{
			get { return JobMessageTypeList.Codes.Import; }
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			EntryHeaderInternal = GetNewEntryHeader();
			base.SetUp();
		}

		protected virtual T GetNewEntryHeader()
		{
			return (T)Factory.New<BaseJobDeclaration>().CustomsEntryHeaders.AddNew();
		}

		#endregion
	}
}
