using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCTOCusHAWB))]
	sealed class DocCTOCusHAWBTest : DocumentWrapperTestCase
	{
		#region TestConsignorPostalAddress

		public void TestConsignorPostalAddress()
		{
			Hawb.CS_ConsignorName = "Name";
			Hawb.CS_ConsignorStreet = "Street";
			Hawb.CS_ConsignorStreet2 = "Street2";
			Hawb.CS_ConsignorCity = "City";
			Hawb.CS_ConsignorState = "State";
			Hawb.CS_ConsignorPostcode = "Post";
			Hawb.CS_RN_NKConsignorCountry = "AU";

			ZString expectedValue = "NAME\nSTREET\nSTREET2\nCITY STATE POST\nAUSTRALIA";
			AssertEquals(expectedValue, Wrapper.ConsignorPostalAddress);
		}

		#endregion

		#region TestConsigneePostalAddress

		public void TestConsigneePostalAddress()
		{
			Hawb.CS_ConsigneeName = "Name";
			Hawb.CS_ConsigneeStreet = "Street";
			Hawb.CS_ConsigneeStreet2 = "Street2";
			Hawb.CS_ConsigneeCity = "City";
			Hawb.CS_ConsigneeState = "State";
			Hawb.CS_ConsigneePostcode = "Post";
			Hawb.CS_RN_NKConsigneeCountry = "AU";

			ZString expectedValue = "NAME\nSTREET\nSTREET2\nCITY STATE POST\nAUSTRALIA";
			AssertEquals(expectedValue.Replace("\n", "\\n").Replace(' ', '-'), Wrapper.ConsigneePostalAddress.Replace("\n", "\\n").Replace(' ', '-'));
		}

		#endregion

		#region Implementation

		#region Hawb

		CTOCusHAWB Hawb
		{
			get
			{
				if (hawb == null)
				{
					hawb = Factory.New<CTOCusHAWB>();
				}

				return hawb;
			}
		}

		CTOCusHAWB hawb;

		#endregion

		#region DocCTOCusHAWB

		DocCTOCusHAWB Wrapper
		{
			get { return DocCTOCusHAWB.New(Hawb, Factory); }
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { Wrapper };
		}

		#endregion
	}
}
