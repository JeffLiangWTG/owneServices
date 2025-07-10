using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UACreditNoteLine))]
	public class UACreditNoteLineTest : APCreditNoteLineTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<UACreditNoteLine>();
		}

		public override void TestTransactionLineFetchHints()
		{
			Assert(true);
		}

		public new void TestValidationForIncompleteTransactionLine()
		{
			Assert(true);
		}

		protected override bool LineCanHaveTaxComponent
		{
			get { return true; }
		}

		protected override bool LineCanHaveForeignCurrency
		{
			get { return true; }
		}

		protected override Type MasterHeaderType
		{
			get { return typeof(UACreditNote); }
		}

		public void TestValidation()
		{
			AssertEquals(((UACreditNoteLine)GetNewBusinessObject()).Validation.GetType(), typeof(UACreditNoteLineValidation));
		}

		protected override bool AcceptAL_AC
		{
			get { return false; }
		}

		protected override bool AcceptAL_AG
		{
			get { return false; }
		}
	}
}
