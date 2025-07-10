using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZBoolDescriptionPairTest : TestCase
	{
		public void TestConstructorSetsDescriptionAndValue()
		{
			Pair = null;

			Pair = new ZBoolDescriptionPair("Unicron", true);
			AssertEquals("Constructor should set Description", "Unicron", Pair.Description);
			Assert("Constructor should set Value", Pair.Value);
		}

		public void TestDescription()
		{
			AssertEquals("Precondition - Description", "Description", Pair.Description);

			Pair.Description = "Optimus Prime";
			AssertEquals("Optimus Prime", Pair.Description);
		}

		public void TestValue()
		{
			Assert("Precondition - Value", !Pair.Value);

			Pair.Value = true;
			Assert(Pair.Value);
		}

		public void TestOnChangedFiredWhenDescriptionSet()
		{
			AssertEquals("Precondition - Description", "Description", Pair.Description);
			Assert("Precondition - OnChangedFired should be false", !OnChangedFired);

			Pair.Description = "Megatron";
			Assert("Pair.OnChanged should have fired", OnChangedFired);

			OnChangedFired = false;
			Pair.Description = "Megatron";
			Assert("Pair.OnChanged should have fired when setting the same description", OnChangedFired);
		}

		public void TestOnChangedFiredWhenValueSet()
		{
			Assert("Precondition - Value", !Pair.Value);
			Assert("Precondition - OnChangedFired should be false", !OnChangedFired);

			Pair.Value = true;
			Assert("Pair.OnChanged should have fired", OnChangedFired);

			OnChangedFired = true;
			Pair.Value = true;
			Assert("Pair.OnChanged should have fired when setting the same value", OnChangedFired);
		}

		public void TestOnChangedFiredWhenPKSet()
		{
			AssertEquals("Precondition - PK", PairWithGuid.PK, TestGuid);
			Assert("Precondition - OnChangedFired should be false", !OnChangedFired);

			ZGuid newTestGuid = ZGuid.NewZGuid();
			Pair.PK = newTestGuid;
			Assert("Pair.OnChanged should have fired", OnChangedFired);

			OnChangedFired = false;
			Pair.PK = newTestGuid;
			Assert("Pair.OnChanged should have fired when setting the same PK", OnChangedFired);
		}

		public void TestGuidPassedIn()
		{
			AssertEquals("The constructor set the GUID properly", PairWithGuid.PK, TestGuid);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			TestGuid = ZGuid.NewZGuid();

			Pair = new ZBoolDescriptionPair("Description", false);
			Pair.OnChanged += new EventHandler(Pair_OnChanged);
			OnChangedFired = false;

			PairWithGuid = new ZBoolDescriptionPair(TestGuid, "Description", false);
			PairWithGuid.OnChanged += new EventHandler(Pair_OnChanged);
			OnChangedFired = false;
		}

		void Pair_OnChanged(object sender, EventArgs e)
		{
			OnChangedFired = true;
		}

		ZGuid TestGuid;
		ZBoolDescriptionPair Pair;
		ZBoolDescriptionPair PairWithGuid;
		bool OnChangedFired;

		#endregion

	}
}
