using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class MiscOptionsControlBag : ControlBag
{
	public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

	[ThreadStatic]
	static MiscOptionsControlBag instance;

	public MiscOptionsControlBag() : base()
	{
		PreClearingCheckBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.PreClearingCheckBox));
		BadgeCodeDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.BadgeCodeDropEdit));
		SubscriberDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.SubscriberDropEdit));
		DefermentAccountNumberDropEdit = RegisterControl(nameof(MiscOptionsLayoutUserControl.DefermentAccountNumberDropEdit));
		SupportingInformationUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.SupportingInformationUserControl));
	}

	public ControlReference PreClearingCheckBox { get; }

	public ControlReference BadgeCodeDropEdit { get; }

	public ControlReference SubscriberDropEdit { get; }

	public ControlReference DefermentAccountNumberDropEdit { get; }

	public ControlReference SupportingInformationUserControl { get; }

	protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();
}

