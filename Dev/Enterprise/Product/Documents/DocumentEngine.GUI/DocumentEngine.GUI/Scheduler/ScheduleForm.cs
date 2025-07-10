using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	/// <summary>
	/// Should be abstract. Can't be abstract so excluded from the Form Bashers instead.
	/// </summary>
#if DEBUG
	[Enterprise.ZArchitecture.GUI.Testing.TestExcludeZWinFormsAllHaveFormBashers]
#endif
	public partial class ScheduleForm : ScheduleFormBase
	{
		public ScheduleForm()
		{
			InitializeComponent();
		}

		public ScheduleForm(Schedule businessEntity)
			: base(businessEntity.Clone())
		{
			InitializeComponent();
			this.OriginalSchedule = businessEntity;
		}

		public new Schedule BusinessEntity
		{
			get { return (Schedule)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return FormVerbs.Edit; }
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			Clear();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			HandleOKButton();
		}

		void Clear()
		{
			OriginalSchedule.Clear();
			OriginalSchedule.ClearDate();
			Close();
		}

		void HandleOKButton()
		{
			ValidateAll(ValidationType.Light);
			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				OriginalSchedule.CopyChangesFrom(BusinessEntity);
				Close();
			}
		}

		internal readonly Schedule OriginalSchedule;
	}

	/// <summary>
	/// This was added in to stop DynamicMocks trying to generate a method override for InitializeComponent().
	/// Excluded as should be deleted when base protected virtual InitializeComponent() is removed.
	/// </summary>
#if DEBUG
	[Enterprise.ZArchitecture.GUI.Testing.TestExcludeZWinFormHasTypedConstructor]
	[Enterprise.ZArchitecture.GUI.Testing.TestExcludeZWinFormsAllHaveFormBashers]
#endif
	public class ScheduleFormBase : ZChildForm
	{
		public ScheduleFormBase()
		{
		}

		public ScheduleFormBase(BusinessObject businessEntity)
			: base(businessEntity)
		{
		}

		protected sealed override void InitializeComponent()
		{
			base.InitializeComponent();
		}
	}
}
