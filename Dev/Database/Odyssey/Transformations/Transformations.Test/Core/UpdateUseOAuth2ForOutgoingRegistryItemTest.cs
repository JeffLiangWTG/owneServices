using System;
using System.Text;
using System.Threading;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing
{
	[TestedType(typeof(UpdateUseOAuth2ForOutgoingRegistryItem))]
	class UpdateUseOAuth2ForOutgoingRegistryItemTest : DataTransformationTestCase
	{
		public void TestTransformationResults_Empty()
		{
			Instance.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertOutgoingRegistryValueEqualsMs365(0);
		}

		public void TestTransformationResults_Outgoing()
		{
			Helper.InsertStmDataRow(UseOAuth2ForOutgoing, "BOL", null);
			Instance.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertOutgoingRegistryValueEqualsMs365(0);
			Helper.DeleteStmDataRow(UseOAuth2ForOutgoing);

			Helper.InsertStmDataRow(UseOAuth2ForOutgoing, Guid.Empty, Guid.Empty, true);
			Instance.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertOutgoingRegistryValueEqualsMs365(1);
			Helper.DeleteStmDataRow(UseOAuth2ForOutgoing);

			Helper.InsertStmDataRow(UseOAuth2ForOutgoing, Guid.Empty, Guid.Empty, false);
			Instance.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertOutgoingRegistryValueEqualsMs365(0);
			Helper.DeleteStmDataRow(UseOAuth2ForOutgoing);
		}

		void AssertOutgoingRegistryValueEqualsMs365(int expectCount)
		{
			var bytes = Helper.GetStmDataValue(UseOAuth2ForOutgoing);
			var actualCount = 0;
			if (bytes != null && Encoding.Unicode.GetString(bytes).Equals("Ms365"))
			{
				actualCount++;
			}
			AssertEquals(expectCount, actualCount);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateUseOAuth2ForOutgoingRegistryItem();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new RegistryTransformationTestHelper();
			Instance = new UpdateUseOAuth2ForOutgoingRegistryItem();
		}

		protected RegistryTransformationTestHelper Helper;
		protected UpdateUseOAuth2ForOutgoingRegistryItem Instance;

		const string UseOAuth2ForOutgoing = "UseOAuth2ForOutgoing";
	}
}
