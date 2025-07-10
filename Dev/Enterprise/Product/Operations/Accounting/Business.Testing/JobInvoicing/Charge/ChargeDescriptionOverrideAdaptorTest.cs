using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ChargeDescriptionOverrideAdaptor))]
	public class ChargeDescriptionOverrideAdaptorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestChargeCollection()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge1 = job.Charges.AddNew();
			var charge2 = job.Charges.AddNew();

			var adaptor = new ChargeDescriptionOverrideAdaptor(Factory);
			adaptor.AddToWrappedObjects(new Charge[] { charge1 });
			AssertEquals("ChargeCollection.Count", 1, adaptor.SelectedWrappedCharges.Count);
			AssertEquals("Charge 1", charge1.PK, adaptor.SelectedWrappedCharges[0].ChargePK);

			adaptor.AddToWrappedObjects(new Charge[] { charge2 });
			AssertEquals("ChargeCollection.Count", 1, adaptor.SelectedWrappedCharges.Count);
			AssertEquals("Charge 2", charge2.PK, adaptor.SelectedWrappedCharges[0].ChargePK);

			adaptor.AddToWrappedObjects(new Charge[] { charge1, charge2 });
			AssertEquals("ChargeCollection.Count", 2, adaptor.SelectedWrappedCharges.Count);
			AssertEquals("Charge 1", charge1.PK, adaptor.SelectedWrappedCharges[0].ChargePK);
			AssertEquals("Charge 2", charge2.PK, adaptor.SelectedWrappedCharges[1].ChargePK);
		}

		public void TestAppendText()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var job = objectCreator.CreateJob("J0001", objectCreator.LocalClient, 1.0M, objectCreator.Agent, 1.0M);
			var charge1 = objectCreator.CreateCharge(job, objectCreator.CC1, 20M, 25M);
			charge1.JR_Desc = objectCreator.CC1.AC_Desc;
			var charge2 = objectCreator.CreateCharge(job, objectCreator.CC2, 30M, 35M);
			charge2.JR_Desc = objectCreator.CC2.AC_Desc;

			var adaptor = new ChargeDescriptionOverrideAdaptor(Factory);
			adaptor.AddToWrappedObjects(new Charge[] { charge1, charge2 });
			AssertEquals("Wrapped ChargeCollection.Count", 2, adaptor.SelectedWrappedCharges.Count);
			AssertEquals("Wrapped Charge 1", charge1.PK, adaptor.SelectedWrappedCharges[0].ChargePK);
			AssertEquals("Wrapped Charge 2", charge2.PK, adaptor.SelectedWrappedCharges[1].ChargePK);

			AssertEquals("Before Appendeding Text  : Wrapped Charge 1 TextToAppend", string.Empty, adaptor.SelectedWrappedCharges[0].TextToAppend);
			AssertEquals("Before Appendeding Text  : Wrapped Charge 2 TextToAppend", string.Empty, adaptor.SelectedWrappedCharges[1].TextToAppend);

			AssertEquals("Before Appendeding Text  : Wrapped Charge 1 OriginalDescription", objectCreator.CC1.AC_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("Before Appendeding Text  : Wrapped Charge 2 OriginalDescription", objectCreator.CC2.AC_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);
			AssertEquals("Before Appendeding Text  : Wrapped Charge 1 OriginalDescription", charge1.JR_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("Before Appendeding Text  : Wrapped Charge 2 OriginalDescription", charge2.JR_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);

			AssertEquals("Before Appendeding Text  : Wrapped Charge 1 FullDescription", charge1.JR_Desc, adaptor.SelectedWrappedCharges[0].JR_Desc);
			AssertEquals("Before Appendeding Text  : Wrapped Charge 2 FullDescription", charge2.JR_Desc, adaptor.SelectedWrappedCharges[1].JR_Desc);

			adaptor.SelectedWrappedCharges[0].TextToAppend = " - Appended To Charge 1";
			adaptor.SelectedWrappedCharges[1].TextToAppend = " - Appended To Charge 2";

			AssertEquals("Appendeding Text : Wrapped Charge 1 TextToAppend", " - Appended To Charge 1", adaptor.SelectedWrappedCharges[0].TextToAppend);
			AssertEquals("Appendeding Text : Wrapped Charge 2 TextToAppend", " - Appended To Charge 2", adaptor.SelectedWrappedCharges[1].TextToAppend);

			AssertEquals("Appendeding Text : Wrapped Charge 1 OriginalDescription", objectCreator.CC1.AC_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("Appendeding Text : Wrapped Charge 2 OriginalDescription", objectCreator.CC2.AC_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);

			AssertEquals("Appendeding Text : Wrapped Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - Appended To Charge 1", adaptor.SelectedWrappedCharges[0].JR_Desc);
			AssertEquals("Appendeding Text : Wrapped Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - Appended To Charge 2", adaptor.SelectedWrappedCharges[1].JR_Desc);
			AssertEquals("Appendeding Text : Job Charge 1 Full Desc. Should remain unchanged as appended text hasn't been applied yet", objectCreator.CC1.AC_Desc, charge1.JR_Desc);
			AssertEquals("Appendeding Text : Job Charge 2 Full Desc. Should remain unchanged as appended text hasn't been applied yet", objectCreator.CC2.AC_Desc, charge2.JR_Desc);

			adaptor.ApplyOrCancelChanges(true);

			AssertEquals("After Applying Appended Text(1) : Job Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - Appended To Charge 1", charge1.JR_Desc);
			AssertEquals("After Applying Appended Text(1) : Job Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - Appended To Charge 2", charge2.JR_Desc);

			adaptor.SelectedWrappedCharges[0].TextToAppend = " - w/o saving 1";
			adaptor.SelectedWrappedCharges[1].TextToAppend = " - w/o saving 2";

			adaptor.ApplyOrCancelChanges(true);

			AssertEquals("After Applying Appended Text (2) : Job Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - w/o saving 1", charge1.JR_Desc);
			AssertEquals("After Applying Appended Text (2) : Job Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - w/o saving 2", charge2.JR_Desc);

			adaptor.SelectedWrappedCharges[0].TextToAppend = " - Again w/o saving 1";
			adaptor.SelectedWrappedCharges[1].TextToAppend = " - Again w/o saving 2";

			AssertEquals("After Applying Appended Text (3) : Job Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - w/o saving 1", charge1.JR_Desc);
			AssertEquals("After Applying Appended Text (3) : Job Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - w/o saving 2", charge2.JR_Desc);
			AssertEquals("After Applying Appended Text (3) : Wrapped Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - Again w/o saving 1", adaptor.SelectedWrappedCharges[0].JR_Desc);
			AssertEquals("After Applying Appended Text (3) : Wrapped Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - Again w/o saving 2", adaptor.SelectedWrappedCharges[1].JR_Desc);

			adaptor.ApplyOrCancelChanges(true);

			Factory.Save();

			AssertEquals("After Save : Charge 1 OriginalDescription", charge1.JR_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("After Save : Charge 2 OriginalDescription", charge2.JR_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);
			AssertEquals("After Save : Charge 1 TextToAppend", ZString.Empty, adaptor.SelectedWrappedCharges[0].TextToAppend);
			AssertEquals("After Save : Charge 2 TextToAppend", ZString.Empty, adaptor.SelectedWrappedCharges[1].TextToAppend);

			//attempting to append again
			adaptor.SelectedWrappedCharges[0].TextToAppend = " - Appended a bit more 1";
			adaptor.SelectedWrappedCharges[1].TextToAppend = " - Appended a bit more 2";

			adaptor.ApplyOrCancelChanges(true);
			AssertEquals("After Applying Appended Text : Job Charge 1 Full Desc (text appended)", objectCreator.CC1.AC_Desc + " - Again w/o saving 1" + " - Appended a bit more 1", charge1.JR_Desc);
			AssertEquals("After Applying Appended Text : Job Charge 2 Full Desc (text appended)", objectCreator.CC2.AC_Desc + " - Again w/o saving 2" + " - Appended a bit more 2", charge2.JR_Desc);

			adaptor.SelectedWrappedCharges[0].TextToAppend = " - Again Appended a bit more 1";
			adaptor.SelectedWrappedCharges[1].TextToAppend = " - Again Appended a bit more 2";

			adaptor.ApplyOrCancelChanges(true);
			AssertEquals("After Applying Appended Text (2) : Job Charge 1 Full Desc (text appended)", objectCreator.CC1.AC_Desc + " - Again w/o saving 1" + " - Again Appended a bit more 1", charge1.JR_Desc);
			AssertEquals("After Applying Appended Text (2) : Job Charge 2 Full Desc (text appended)", objectCreator.CC2.AC_Desc + " - Again w/o saving 2" + " - Again Appended a bit more 2", charge2.JR_Desc);

			Factory.Save();

			AssertEquals("After Save : Charge 1 OriginalDescription", charge1.JR_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("After Save : Charge 2 OriginalDescription", charge2.JR_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);
		}

		public void TestCancelAppendText()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var job = objectCreator.CreateJob("J0001", objectCreator.LocalClient, 1.0M, objectCreator.Agent, 1.0M);
			var charge1 = objectCreator.CreateCharge(job, objectCreator.CC1, 20M, 25M);
			charge1.JR_Desc = objectCreator.CC1.AC_Desc;
			var charge2 = objectCreator.CreateCharge(job, objectCreator.CC2, 30M, 35M);
			charge2.JR_Desc = objectCreator.CC2.AC_Desc;

			var adaptor = new ChargeDescriptionOverrideAdaptor(Factory);
			adaptor.AddToWrappedObjects(new Charge[] { charge1, charge2 });
			AssertEquals("ChargeCollection.Count", 2, adaptor.SelectedWrappedCharges.Count);
			AssertEquals("Charge 1", charge1.PK, adaptor.SelectedWrappedCharges[0].ChargePK);
			AssertEquals("Charge 2", charge2.PK, adaptor.SelectedWrappedCharges[1].ChargePK);

			AssertEquals("Wrapped Charge 1 OriginalDescription", objectCreator.CC1.AC_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("Wrapped Charge 2 OriginalDescription", objectCreator.CC2.AC_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);
			AssertEquals("Wrapped Charge 1 TextToAppend", string.Empty, adaptor.SelectedWrappedCharges[0].TextToAppend);
			AssertEquals("Wrapped Charge 2 TextToAppend", string.Empty, adaptor.SelectedWrappedCharges[1].TextToAppend);

			adaptor.SelectedWrappedCharges[0].TextToAppend = " - Appended To Charge 1";
			adaptor.SelectedWrappedCharges[1].TextToAppend = " - Appended To Charge 2";

			AssertEquals("Wrapped Charge 1 OriginalDescription", objectCreator.CC1.AC_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("Wrapped Charge 2 OriginalDescription", objectCreator.CC2.AC_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);
			AssertEquals("Wrapped Charge 1 TextToAppend", " - Appended To Charge 1", adaptor.SelectedWrappedCharges[0].TextToAppend);
			AssertEquals("Wrapped Charge 2 TextToAppend", " - Appended To Charge 2", adaptor.SelectedWrappedCharges[1].TextToAppend);
			AssertEquals("Wrapped Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - Appended To Charge 1", adaptor.SelectedWrappedCharges[0].JR_Desc);
			AssertEquals("Wrapped Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - Appended To Charge 2", adaptor.SelectedWrappedCharges[1].JR_Desc);
			AssertEquals("Job Charge 1 Full Desc", objectCreator.CC1.AC_Desc, charge1.JR_Desc);
			AssertEquals("Job Charge 2 Full Desc", objectCreator.CC2.AC_Desc, charge2.JR_Desc);

			adaptor.ApplyOrCancelChanges(false);
			AssertEquals("Wrapped Charge 1 TextToAppend", ZString.Empty, adaptor.SelectedWrappedCharges[0].TextToAppend);
			AssertEquals("Wrapped Charge 2 TextToAppend", ZString.Empty, adaptor.SelectedWrappedCharges[1].TextToAppend);
			AssertEquals("Wrapped Charge 1 OriginalDescription", objectCreator.CC1.AC_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("Wrapped Charge 2 OriginalDescription", objectCreator.CC2.AC_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);
			AssertEquals("Wrapped Charge 1 Full Desc", objectCreator.CC1.AC_Desc, adaptor.SelectedWrappedCharges[0].JR_Desc);
			AssertEquals("Wrapped Charge 2 Full Desc", objectCreator.CC2.AC_Desc, adaptor.SelectedWrappedCharges[1].JR_Desc);
			AssertEquals("Job Charge 1 Full Desc", objectCreator.CC1.AC_Desc, charge1.JR_Desc);
			AssertEquals("Job Charge 2 Full Desc", objectCreator.CC2.AC_Desc, charge2.JR_Desc);

			adaptor.SelectedWrappedCharges[0].TextToAppend = " - attmp 2: Appended To Charge 1";
			adaptor.SelectedWrappedCharges[1].TextToAppend = " - attmp 2: Appended To Charge 2";

			adaptor.ApplyOrCancelChanges(true);
			AssertEquals("Wrapped Charge 1 TextToAppend", " - attmp 2: Appended To Charge 1", adaptor.SelectedWrappedCharges[0].TextToAppend);
			AssertEquals("Wrapped Charge 2 TextToAppend", " - attmp 2: Appended To Charge 2", adaptor.SelectedWrappedCharges[1].TextToAppend);
			AssertEquals("Wrapped Charge 1 OriginalDescription", objectCreator.CC1.AC_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("Wrapped Charge 2 OriginalDescription", objectCreator.CC2.AC_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);
			AssertEquals("Wrapped Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - attmp 2: Appended To Charge 1", adaptor.SelectedWrappedCharges[0].JR_Desc);
			AssertEquals("Wrapped Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - attmp 2: Appended To Charge 2", adaptor.SelectedWrappedCharges[1].JR_Desc);
			AssertEquals("Job Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - attmp 2: Appended To Charge 1", charge1.JR_Desc);
			AssertEquals("Job Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - attmp 2: Appended To Charge 2", charge2.JR_Desc);

			adaptor.SelectedWrappedCharges[0].TextToAppend = " - attmp 3: Appended To Charge 1";
			adaptor.SelectedWrappedCharges[1].TextToAppend = " - attmp 3: Appended To Charge 2";

			adaptor.ApplyOrCancelChanges(false);
			AssertEquals("Wrapped Charge 1 TextToAppend", " - attmp 2: Appended To Charge 1", adaptor.SelectedWrappedCharges[0].TextToAppend);
			AssertEquals("Wrapped Charge 2 TextToAppend", " - attmp 2: Appended To Charge 2", adaptor.SelectedWrappedCharges[1].TextToAppend);
			AssertEquals("Wrapped Charge 1 OriginalDescription", objectCreator.CC1.AC_Desc, adaptor.SelectedWrappedCharges[0].OriginalDescription);
			AssertEquals("Wrapped Charge 2 OriginalDescription", objectCreator.CC2.AC_Desc, adaptor.SelectedWrappedCharges[1].OriginalDescription);
			AssertEquals("Wrapped Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - attmp 2: Appended To Charge 1", adaptor.SelectedWrappedCharges[0].JR_Desc);
			AssertEquals("Wrapped Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - attmp 2: Appended To Charge 2", adaptor.SelectedWrappedCharges[1].JR_Desc);
			AssertEquals("Job Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - attmp 2: Appended To Charge 1", charge1.JR_Desc);
			AssertEquals("Job Charge 2 Full Desc", objectCreator.CC2.AC_Desc + " - attmp 2: Appended To Charge 2", charge2.JR_Desc);

			charge2.Delete();
			adaptor.SelectedWrappedCharges[0].TextToAppend = " - Appended To Charge 1 Again";

			adaptor.ApplyOrCancelChanges(true);
			AssertEquals("Job Charge 1 Full Desc", objectCreator.CC1.AC_Desc + " - Appended To Charge 1 Again", charge1.JR_Desc);
			AssertEquals("ChargeCollection.Count", 1, adaptor.SelectedWrappedCharges.Count);
			AssertEquals("Charge 1", charge1.PK, adaptor.SelectedWrappedCharges[0].ChargePK);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			var chargePKs = new ZGuid[] { charge.PK };
			return new ChargeDescriptionOverrideAdaptor(Factory);
		}
	}
}
