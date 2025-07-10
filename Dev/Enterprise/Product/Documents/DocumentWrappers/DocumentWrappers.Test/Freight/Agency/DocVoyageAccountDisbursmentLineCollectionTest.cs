using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocVoyageAccountDisbursementLineCollection))]
	sealed class DocVoyageAccountDisbursmentLineCollectionTest : DocBaseWrapperCollectionTest<DocVoyageAccountDisbursementLineCollection>
	{
		public void TestNewWithSingleBranch()
		{
			CodeDescriptionPairList pairList = new CodeDescriptionPairList();
			pairList.AddPair("ALP", "Alpha");
			pairList.AddPair("BET", "Beta");

			LinerAgencyDataRegistry.Instance.DisbursementSubGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pairList);

			Charge[] charges = new Charge[]
			{
				CreateCharge(XDA1, Branch1, 10101),
				CreateCharge(XDA2, Branch1, 20202),
				CreateCharge(XDB1, Branch1, 40404),
				CreateCharge(XDB1, Branch1, 80808),
				CreateCharge(XFA1, Branch1, 161616),
			};

			DocVoyageAccountDisbursementLineCollection collection = DocVoyageAccountDisbursementLineCollection.New(charges, Factory);

			AssertContainsExactElementsInAnyOrder("",
				new string[]
				{
					"Group: 'Alpha'\r\n" +
					"Description: 'X Disbursment Alpha 1'\r\n" +
					"Amount[BN1]: 10,101.00 ERN\r\n" +
					"",

					"Group: 'Alpha'\r\n" +
					"Description: 'X Disbursment Alpha 2'\r\n" +
					"Amount[BN1]: 20,202.00 ERN\r\n" +
					"",

					"Group: 'Beta'\r\n" +
					"Description: 'X Disbursment Beta 1'\r\n" +
					"Amount[BN1]: 121,212.00 ERN\r\n" +
					"",

					"Group: 'Other'\r\n" +
					"Description: 'X Freight 1'\r\n" +
					"Amount[BN1]: 161,616.00 ERN\r\n" +
					"",
				},
				Array.ConvertAll(
					collection.ToArray<DocVoyageAccountDisbursementLine>(), RenderLine));
		}

		public void TestNewWithMultipleBranches()
		{
			CodeDescriptionPairList pairList = new CodeDescriptionPairList();
			pairList.AddPair("ALP", "Alpha");
			pairList.AddPair("BET", "Beta");

			LinerAgencyDataRegistry.Instance.DisbursementSubGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pairList);

			Charge[] charges = new Charge[]
			{
				CreateCharge(XDA1, Branch1, 1),
				CreateCharge(XDA2, Branch1, 2),
				CreateCharge(XDB1, Branch1, 4),
				CreateCharge(XDB1, Branch1, 8),
				CreateCharge(XFA1, Branch1, 16),

				CreateCharge(XDA1, Branch2, 100),
				CreateCharge(XDA2, Branch2, 200),
				CreateCharge(XDB1, Branch2, 400),
				CreateCharge(XDB1, Branch2, 800),
				CreateCharge(XFA1, Branch2, 1600),

				CreateCharge(XDA1, Branch3, 10000),
				CreateCharge(XDA2, Branch3, 20000),
				CreateCharge(XDB1, Branch3, 40000),
				CreateCharge(XDB1, Branch3, 80000),
				CreateCharge(XFA1, Branch3, 160000),
			};

			DocVoyageAccountDisbursementLineCollection collection = DocVoyageAccountDisbursementLineCollection.New(charges, Factory);

			AssertContainsExactElementsInAnyOrder("",
				new string[]
				{
					"Group: 'Alpha'\r\n" +
					"Description: 'X Disbursment Alpha 1'\r\n" +
					"Amount[BN1]: 1.00 ERN\r\n" +
					"Amount[BN2]: 100.00 ERN\r\n" +
					"Amount[BN3]: 10,000.00 ERN\r\n" +
					"Amount[Total]: 10,101.00 ERN\r\n" +
					"",

					"Group: 'Alpha'\r\n" +
					"Description: 'X Disbursment Alpha 2'\r\n" +
					"Amount[BN1]: 2.00 ERN\r\n" +
					"Amount[BN2]: 200.00 ERN\r\n" +
					"Amount[BN3]: 20,000.00 ERN\r\n" +
					"Amount[Total]: 20,202.00 ERN\r\n" +
					"",

					"Group: 'Beta'\r\n" +
					"Description: 'X Disbursment Beta 1'\r\n" +
					"Amount[BN1]: 12.00 ERN\r\n" +
					"Amount[BN2]: 1,200.00 ERN\r\n" +
					"Amount[BN3]: 120,000.00 ERN\r\n" +
					"Amount[Total]: 121,212.00 ERN\r\n" +
					"",

					"Group: 'Other'\r\n" +
					"Description: 'X Freight 1'\r\n" +
					"Amount[BN1]: 16.00 ERN\r\n" +
					"Amount[BN2]: 1,600.00 ERN\r\n" +
					"Amount[BN3]: 160,000.00 ERN\r\n" +
					"Amount[Total]: 161,616.00 ERN\r\n" +
					"",
				},
				Array.ConvertAll(
					collection.ToArray<DocVoyageAccountDisbursementLine>(), RenderLine));
		}

		public void TestHeaderIndexWithSingleBranch()
		{
			Charge[] charges = new Charge[]
			{
				CreateCharge(XDA1, Branch1, 100),
			};

			DocVoyageAccountDisbursementLineCollection collection = DocVoyageAccountDisbursementLineCollection.New(charges, Factory);

			AssertMultilineASCIIEquals("",
					"Group: ''\r\n" +
					"Description: ''\r\n" +
					"Amount[BN1]: 1.00 ERN\r\n" +
					"",
					RenderLine(collection["heading"]));
		}

		public void TestHeaderIndexWithMultipleBranches()
		{
			Charge[] charges = new Charge[]
			{
				CreateCharge(XDA1, Branch1, 100),
				CreateCharge(XDA1, Branch2, 200),
				CreateCharge(XDA1, Branch3, 400),
			};

			DocVoyageAccountDisbursementLineCollection collection = DocVoyageAccountDisbursementLineCollection.New(charges, Factory);

			AssertMultilineASCIIEquals("",
					"Group: ''\r\n" +
					"Description: ''\r\n" +
					"Amount[BN1]: 1.00 ERN\r\n" +
					"Amount[BN2]: 1.00 ERN\r\n" +
					"Amount[BN3]: 1.00 ERN\r\n" +
					"Amount[Total]: 1.00 ERN\r\n" +
					"",
					RenderLine(collection["heading"]));
		}

		#region Implementation

		GlbBranch Branch1
		{
			get
			{
				if (branch1 == null)
				{
					GlbCompany company = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
					branch1 = company.Branches.AddNew();
					branch1.GB_Code = "BN1";
				}
				return branch1;
			}
		}
		GlbBranch branch1;

		GlbBranch Branch2
		{
			get
			{
				if (branch2 == null)
				{
					GlbCompany company = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
					branch2 = company.Branches.AddNew();
					branch2.GB_Code = "BN2";
				}
				return branch2;
			}
		}
		GlbBranch branch2;

		GlbBranch Branch3
		{
			get
			{
				if (branch3 == null)
				{
					GlbCompany company = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
					branch3 = company.Branches.AddNew();
					branch3.GB_Code = "BN3";
				}
				return branch3;
			}
		}
		GlbBranch branch3;

		AccChargeCode XDA1
		{
			get
			{
				if (xda1 == null)
				{
					xda1 = Factory.New<AccChargeCode>();
					xda1.AC_Code = "XDA1";
					xda1.AC_Desc = "X Disbursment Alpha 1";
					xda1.AC_ChargeGroup = ChargeCodeGroupList.Codes.ShippingDisbursements;
					xda1.AC_ChargeSubGroup = "ALP";
				}
				return xda1;
			}
		}
		AccChargeCode xda1;

		AccChargeCode XDA2
		{
			get
			{
				if (xda2 == null)
				{
					xda2 = Factory.New<AccChargeCode>();
					xda2.AC_Code = "XDA2";
					xda2.AC_Desc = "X Disbursment Alpha 2";
					xda2.AC_ChargeGroup = ChargeCodeGroupList.Codes.ShippingDisbursements;
					xda2.AC_ChargeSubGroup = "ALP";
				}
				return xda2;
			}
		}
		AccChargeCode xda2;

		AccChargeCode XDB1
		{
			get
			{
				if (xdb1 == null)
				{
					xdb1 = Factory.New<AccChargeCode>();
					xdb1.AC_Code = "XDB1";
					xdb1.AC_Desc = "X Disbursment Beta 1";
					xdb1.AC_ChargeGroup = ChargeCodeGroupList.Codes.ShippingDisbursements;
					xdb1.AC_ChargeSubGroup = "BET";
				}
				return xdb1;
			}
		}
		AccChargeCode xdb1;

		AccChargeCode XFA1
		{
			get
			{
				if (xfa1 == null)
				{
					xfa1 = Factory.New<AccChargeCode>();
					xfa1.AC_Code = "XFA1";
					xfa1.AC_Desc = "X Freight 1";
					xfa1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
					xfa1.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.CustomsHold;
				}
				return xfa1;
			}
		}
		AccChargeCode xfa1;

		Charge CreateCharge(AccChargeCode chargeCode, GlbBranch branch, ZDecimal localAmount)
		{
			Charge charge = Factory.New<Charge>();
			charge.JR_AC = chargeCode.PK;
			charge.JR_GB = branch.PK;
			charge.JR_LocalSellAmt = localAmount;

			return charge;
		}

		string RenderLine(DocVoyageAccountDisbursementLine line)
		{
			if (line == null)
			{
				return "<null>";
			}
			else
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendFormat("Group: '{0}'\r\nDescription: '{1}'\r\n", line.Group, line.Description);
				foreach (DocVoyageAccountDisbursementAmount amount in line.Amounts)
				{
					builder.AppendFormat("Amount[{0}]: {1}\r\n", amount.Title, amount.LocalAmount.AmountAndCurrencyCode);
				}
				return builder.ToString();
			}
		}

		protected override DocVoyageAccountDisbursementLineCollection GetNewDocumentWrapperCollection()
		{
			return DocVoyageAccountDisbursementLineCollection.New(Array.Empty<Charge>(), Factory);
		}

		protected override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			DocVoyageAccountDisbursementLine line = DocVoyageAccountDisbursementLine.New("group", "description", Factory);
			collection.Add(line);
			return line;
		}

		protected override object GetNewObjectToWrap()
		{
			return null;
		}

		#endregion
	}
}
