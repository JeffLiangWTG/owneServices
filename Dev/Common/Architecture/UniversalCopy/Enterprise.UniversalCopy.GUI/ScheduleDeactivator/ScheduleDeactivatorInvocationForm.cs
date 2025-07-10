using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.UniversalCopy.GUI
{
	[CodeAlive("This class will be used in future workitems")]
	public partial class ScheduleDeactivatorInvocationForm : ZChildForm, IScheduleDeactivatorView
	{
		public ScheduleDeactivatorInvocationForm(ScheduleDeactivatorViewModel viewModel)
			: base(viewModel)
		{
			InitializeComponent();
			this.viewModel = viewModel;

			Build();
		}

		readonly ScheduleDeactivatorViewModel viewModel;

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		internal bool HasReceivedResponse { get; private set; }

		public ScheduleDeactivatorResponse GetResponseFromUser()
		{
			ZFormModaliser.ShowDialogAndDispose(this);

			return viewModel.Response ?? ScheduleDeactivatorResponse.DoNotCancelOrDeactivate;
		}

		void Build()
		{
			var buttonStrip = new MultiActionProvidingButtonStrip<ScheduleDeactivatorResponse>(GetButtonActions().ToArray());
			buttonStripPanel.Controls.Add(buttonStrip);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!HasReceivedResponse)
			{
				viewModel.Response = ScheduleDeactivatorResponse.DoNotCancelOrDeactivate;
			}

			base.OnClosing(e);
		}

		IEnumerable<ButtonStripAction<ScheduleDeactivatorResponse>> GetButtonActions()
		{
			yield return GetButton(Res.GetString("501c03a7-770b-4b13-ae16-c76116cb20ac", "Cancel and deactivate"), ResString.GetMultilingualString("95908289-cf44-463d-acb4-1ac2fb21e4fa", "Cancels job and deactivates all associated copy schedules"), ScheduleDeactivatorResponse.CancelAndDeactivate);
			yield return GetButton(Res.GetString("4615fbd2-1dfd-4eea-a0a2-540377ab4f7e", "Cancel and do not deactivate"), ResString.GetMultilingualString("3100618f-e488-41fb-9b79-1c99f7933d1e", "Cancels job but does not deactivate the shown copy schedules"), ScheduleDeactivatorResponse.CancelAndDoNotDeactivate);
			yield return GetButton(Res.GetString("a2ded1e0-6a6c-46ae-8f57-428ecdceb051", "Do not cancel or deactivate"), ResString.GetMultilingualString("2acc0700-6070-4e53-8686-231518ad10e6", "Does not do anything"), ScheduleDeactivatorResponse.DoNotCancelOrDeactivate);
		}

		ButtonStripAction<ScheduleDeactivatorResponse> GetButton(string text, MultilingualString tooltip, ScheduleDeactivatorResponse response)
		{
			var buttonAction = new ButtonStripAction<ScheduleDeactivatorResponse>
			{
				Text = text,
				Response = response,
				ToolTip = tooltip,
				FireAction = (s, e) => PerformResponse(response),
			};

			return buttonAction;
		}

		void PerformResponse(ScheduleDeactivatorResponse response)
		{
			viewModel.Response = response;
			HasReceivedResponse = true;
			Close();
		}
	}
}
