using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRRespondeeWrapperTest : TestCaseWithFactory
	{
		public void TestMessagesForDeclaration()
		{
			AssertEquals(declaration.Messages, cMRRespondeeWrapperDeclaration.Messages);
		}

		public void TestMessagesForConsol()
		{
			AssertEquals(consol.Messages, cMRRespondeeWrapperConsol.Messages);
		}

		public void TestMessagesForShipment()
		{
			AssertEquals(shipment.Messages, cMRRespondeeWrapperShipment.Messages);
		}

		public void TestMessagesForJobVoyage()
		{
			AssertEquals(voyage.Messages, cMRRespondeeWrapperVoyage.Messages);
		}

		public void TestMessagesForVoyageDestination()
		{
			AssertEquals(destination.Messages, cMRRespondeeWrapperDestination.Messages);
		}

		public void TestDetailsForDeclaration()
		{
			AssertEquals(declaration.Details, cMRRespondeeWrapperDeclaration.Details);
		}

		public void TestDetailsForConsol()
		{
			AssertEquals(new FreightConsolWrapper(consol).Details, cMRRespondeeWrapperConsol.Details);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestWrapNull()
		{
			CMRRespondeeWrapper nullWrapper = CMRRespondeeWrapper.GetWrapper(null);
			AssertNull("Messages", nullWrapper.Messages);
			AssertEquals("Details", ZString.Empty, nullWrapper.Details);
			AssertEquals("ShortDecription", ZString.Empty, nullWrapper.ShortDescription);
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestWrapUnmatched()
		{
			CMRRespondeeWrapper dummyWrapper = CMRRespondeeWrapper.GetWrapper(Factory.New<DummyBusinessObject>());
			EDIMessageCollection messages = dummyWrapper.Messages;
			//AssertNotNull("Messages", DummyWrapper.Messages);
			//AssertEquals("Dummy is master", DummyWrapper.WrappedObject, DummyWrapper.Messages.Master);
		}

		public void TestMessagesForOrganisation()
		{
			var organisation = Factory.New<OrgHeader>();
			var wrapper = new OrgHeaderWrapper(organisation);
			var cmrRespondeeWrapper = new CMRRespondeeWrapper(organisation);
			AssertNotNull(cmrRespondeeWrapper.Messages);
			AssertEquals("ShortDecription", " ", cmrRespondeeWrapper.ShortDescription);

			organisation.OH_Code = "TST";
			organisation.OH_FullName = "FULLNAME";
			AssertEquals("ShortDecription", "TST FULLNAME", cmrRespondeeWrapper.ShortDescription);
		}

		public void TestShortDescriptionForConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0067382734";
			var wrapper = new FreightConsolWrapper(consol);
			var cmrRespondeeWrapper = new CMRRespondeeWrapper(consol);
			AssertNotNull(cmrRespondeeWrapper.Messages);
			AssertEquals("ShortDecription", "Consol #: C0067382734", cmrRespondeeWrapper.ShortDescription);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			shipment = Factory.New<ForwardingShipment>();
			cMRRespondeeWrapperShipment = new CMRRespondeeWrapper(shipment);
			cMRRespondeeWrapperConsol = new CMRRespondeeWrapper(consol);
			declaration = Factory.New<JobDeclaration>();
			cMRRespondeeWrapperDeclaration = new CMRRespondeeWrapper(declaration);
			voyage = Factory.New<JobVoyage>();
			cMRRespondeeWrapperVoyage = new CMRRespondeeWrapper(voyage);
			destination = Factory.New<VoyageDestination>();
			cMRRespondeeWrapperDestination = new CMRRespondeeWrapper(destination);
		}

		ForwardingConsol consol;
		ForwardingShipment shipment;
		JobDeclaration declaration;
		JobVoyage voyage;
		VoyageDestination destination;
		CMRRespondeeWrapper cMRRespondeeWrapperConsol;
		CMRRespondeeWrapper cMRRespondeeWrapperShipment;
		CMRRespondeeWrapper cMRRespondeeWrapperDeclaration;
		CMRRespondeeWrapper cMRRespondeeWrapperVoyage;
		CMRRespondeeWrapper cMRRespondeeWrapperDestination;

		#endregion

	}
}
