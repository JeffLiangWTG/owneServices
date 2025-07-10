using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using BaseCustomsMessageBuilders = Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class UBMREQMessageBuilderAbstractTest : TestCaseWithFactory
	{
		public void TestDocumentMessageNameCode()
		{
			AssertEquals(DocumentNameCodeList.UnderbondRequest, Builder.DocumentNameCode);
		}

		public void TestDocumentName()
		{
			AssertEquals("UBMREQ", Builder.DocumentName);
		}

		public void TestRequestReason()
		{
			underbond.C4_MovementReason = "XXX";
			Assert(GeneratedMessage.Contains("RFF+ACD:XXX'"));
		}

		public void TestResponsiblePartyClientID()
		{
			Assert(GeneratedMessage.Contains("NAD+VW+" + ABN + "::95'"));
		}

		public void TestUnderbondBySeaMovementVoyageDetails()
		{
			underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel;
			underbond.C4_UnderbondBySeaVoyage = "531";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_LloydsNumber = "12345";
			underbond.C4_UnderbondBySeaVessel = vessel.RV_Code;
			Assert(GeneratedMessage.Contains("TDT+1+531+IVS+++++12345::11'"));
		}

		public void TestModeOfMovement()
		{
			underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel;
			Assert(GeneratedMessage.Contains("TDT+1++" + CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel + "'"));
		}

		public void TestDestinationEstID()
		{
			underbond.C4_DestinationPremiseID = "23456";
			Assert(GeneratedMessage.Contains("LOC+4+23456::95'"));
		}

		public void TestOriginatingEstID()
		{
			underbond.C4_OriginPremiseID = "12345";
			Assert(GeneratedMessage.Contains("LOC+5+12345::95'"));
		}

		public void TestDischargeEstID()
		{
			underbond.C4_IsMoveFromDischarge = true;
			underbond.C4_OriginPremiseID = "654321";
			Assert(GeneratedMessage.Contains("LOC+11+654321::95'"));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new UBMREQMessageBuilder(null);
		}

		public abstract void TestEstimatedDateOfArrival();

		public abstract void TestCreateEndToEnd();

		public abstract void TestWithdrawEndToEnd();

		public abstract void TestPopulateTDT();

		protected CusUnderbond underbond;
		protected override void SetUp()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.PrimaryRegistrationNumber.Number = ABN;
			base.SetUp();
			underbond = Factory.New<CusUnderbond>();
		}

		protected abstract UBMREQMessageBuilder Builder { get; }

		protected BaseCustomsMessageBuilders.MessageSubTypes messageSubType = BaseCustomsMessageBuilders.MessageSubTypes.Create;

		protected ZString GeneratedMessage => Builder.GeneratedMessageStrings[0];

		const string ABN = "41065894724";
	}
}
