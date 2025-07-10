using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI.Balloons;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class TrainingModeExtensionTest : BaseExtensionTest<TrainingModeExtension>
	{
		GenericExtendedControl control;

		Mock<IBalloon> balloonMock;
		IBalloon balloon;
		TestTrainingModeExtension extension;

		protected override void SetUp()
		{
			base.SetUp();

			control = new GenericExtendedControl();

			balloonMock = new Mock<IBalloon>(MockBehavior.Strict);
			balloon = balloonMock.Object;

			extension = new TestTrainingModeExtension(balloon);
			extension.Initialize(control);
		}

		[ExpectNoExceptions]
		public void TestDoNothingIfTrainingModeIsNotEnabled()
		{
			extension.TrainingModeEnabled = false;
			extension.ShowBalloon();
		}

		[ExpectNoExceptions]
		public void TestShowBalloonForOwnerControlWhenTrainingModeIsEnabled()
		{
			extension.TrainingModeEnabled = true;

			balloonMock.Setup(m => m.Show(control));

			extension.ShowBalloon();
		}

		#region Support

		public class TestTrainingModeExtension : TrainingModeExtension
		{
			public bool TrainingModeEnabled;

			public TestTrainingModeExtension(IBalloon balloon)
				: base(balloon)
			{ }

			protected override bool IsTrainingModeEnabled()
			{
				return TrainingModeEnabled;
			}
		}

		#endregion
	}
}
