using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Balloons;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	#region Interface

	public interface ITrainingModeExtension : IControlExtension
	{
		void ShowBalloon();
	}

	#endregion

	public class TrainingModeExtension : ControlExtension, ITrainingModeExtension
	{
		#region Fields

		readonly IBalloon balloon;

		#endregion

		#region Constructors

		public TrainingModeExtension()
			: this(Balloon.Instance)
		{ }

		public TrainingModeExtension(IBalloon balloon)
		{
			this.balloon = balloon;
		}

		#endregion

		#region Mounting

		public override void Initialize(IExtendedControl owner)
		{
			base.Initialize(owner);
			Owner.Host.Enter += OnEnter;
		}

		public override void Dispose()
		{
			if (Owner != null)
			{
				Owner.Host.Enter -= OnEnter;
			}
			base.Dispose();
		}

		#region Control Event Handlers

		void OnEnter(object sender, EventArgs args)
		{
			ShowBalloon();
		}

		#endregion

		#endregion

		#region Implementation

		public void ShowBalloon()
		{
			if (!IsTrainingModeEnabled())
			{
				return;
			}

			balloon.Show(Owner);
		}

		#endregion

		#region Support

		protected virtual bool IsTrainingModeEnabled()
		{
			return EnvProxy.Instance.Registry.TraningModeEnabled;
		}

		#endregion
	}
}
