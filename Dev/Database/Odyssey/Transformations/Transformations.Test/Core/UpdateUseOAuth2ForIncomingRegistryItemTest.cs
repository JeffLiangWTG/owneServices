using System;
using System.Text;
using System.Threading;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing
{
	[TestedType(typeof(UpdateUseOAuth2ForIncomingRegistryItem))]
	class UpdateUseOAuth2ForIncomingRegistryItemTest : DataTransformationTestCase
	{
		public void TestTransformationResults_Empty()
		{
			Instance.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertIncomingRegistryValueEqualsMs365(0);
		}

		public void TestTransformationResults_Incoming()
		{
			Helper.InsertStmDataRow(UseOAuth2ForIncoming, "BOL", null);
			Instance.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertIncomingRegistryValueEqualsMs365(0);
			Helper.DeleteStmDataRow(UseOAuth2ForIncoming);

			Helper.InsertStmDataRow(UseOAuth2ForIncoming, Guid.Empty, Guid.Empty, true);
			Instance.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertIncomingRegistryValueEqualsMs365(1);
			Helper.DeleteStmDataRow(UseOAuth2ForIncoming);

			Helper.InsertStmDataRow(UseOAuth2ForIncoming, Guid.Empty, Guid.Empty, false);
			Instance.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertIncomingRegistryValueEqualsMs365(0);
			Helper.DeleteStmDataRow(UseOAuth2ForIncoming);
		}

		void AssertIncomingRegistryValueEqualsMs365(int expectCount)
		{
			var bytes = Helper.GetStmDataValue(UseOAuth2ForIncoming);
			var actualCount = 0;
			if (bytes != null && Encoding.Unicode.GetString(bytes).Equals("Ms365"))
			{
				actualCount++;
			}
			AssertEquals(expectCount, actualCount);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateUseOAuth2ForIncomingRegistryItem();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new RegistryTransformationTestHelper();
			Instance = new UpdateUseOAuth2ForIncomingRegistryItem();
		}

		protected RegistryTransformationTestHelper Helper;
		protected UpdateUseOAuth2ForIncomingRegistryItem Instance;

		const string UseOAuth2ForIncoming = "UseOAuth2ForIncoming";
	}
}
