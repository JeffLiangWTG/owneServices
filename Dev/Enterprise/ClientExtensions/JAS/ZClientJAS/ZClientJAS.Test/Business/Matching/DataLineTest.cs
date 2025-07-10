using CargoWise.Types;
using Enterprise.Client.JAS.Business.Matching.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Matching
{
	class DataLineTest : BaseDataLineTest
	{
		public void TestCurrentSubsidiary()
		{
			EnsureNettingCodeIsSet();
			ZString expected = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetUNC();
			AssertEquals(expected, Line.CurrentSubsidiary);
		}

		public void TestBusinessObjectFactory()
		{
			AssertNotNull(Line.Factory);
		}

		#region Implementation
		protected override void SetUp()
		{
			Line = new DummyDataLine();
		}

		void EnsureNettingCodeIsSet()
		{
			if (GlbCompany.CurrentCompany.OrgProxy == null)
			{
				OrgHeader org = Factory.New<OrgHeader>();
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
				Factory.Save();
				AssertNotNull("Org Proxy should not be null", GlbCompany.CurrentCompany.OrgProxy);
			}

			OrgHeader orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			ZString uNC = orgProxy.CustomsCodes.GetUNC();
			if (uNC.IsEmpty)
			{
				SetNettingCode(orgProxy, "HA123");
			}
		}

		DataLine Line;
		#region DummyDataLine class
		protected class DummyDataLine : DataLine
		{
			public override ZDecimal Amount
			{
				get
				{
					return 0;
				}
			}

			public override ZString Category
			{
				get
				{
					return ZString.Empty;
				}
			}

			public override ZString CounterpartSubsidiary
			{
				get
				{
					return ZString.Empty;
				}
			}

			public override ZBool CreditNote
			{
				get
				{
					return ZBool.False;
				}
			}

			public override ZString CurrencyCode
			{
				get
				{
					return ZString.Empty;
				}
			}

			public override ZString FullInvoiceNumber
			{
				get
				{
					return ZString.Empty;
				}
			}

			public override ZString HouseBill
			{
				get
				{
					return ZString.Empty;
				}
			}

			public override ZDateTime InvoiceDate
			{
				get
				{
					return ZDateTime.Empty;
				}
			}

			public override ZString MasterBill
			{
				get
				{
					return ZString.Empty;
				}
			}

			public override ZDateTime MaturityDate
			{
				get
				{
					return ZDateTime.Empty;
				}
			}

			public override ZString TransactionType
			{
				get
				{
					return ZString.Empty;
				}
			}
		}
		#endregion
		#endregion
	}
}
