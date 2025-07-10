using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class ExportOrientedUnitsLayout : IPanelLayoutProvider
{
	PanelLayout ExportOrientedUnits { get; }

	PanelLayout IPanelLayoutProvider.Layout => ExportOrientedUnits;

	public ExportOrientedUnitsLayout()
	{
		ExportOrientedUnits = CreateExportOrientedUnitsLayout();
	}

	PanelLayout CreateExportOrientedUnitsLayout()
	{
		var builder = new ExportOrientedUnitsLayoutBuilder();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.ExportOrientedUnitsDocAddressControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ExaminationDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ExaminingOfficerNameTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ExaminingOfficerDesignationTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.SupervisingOfficerNameTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.SupervisingOfficerDesignationTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.CommissionerateTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.DivisionTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.RangeTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.SealNoTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VerifiedDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.SampleForwardedDropEdit, ControlWidthClass.Auto);

		builder.AddControlBehaviour<ZDocAddressControl>(commonBag.ExportOrientedUnitsDocAddressControl, (control, declaration) =>
		{
			control.Enabled = !(declaration is JobDeclaration && declaration.IsFactoryStuffed && declaration.IsSeaAndContainerised);
		});
		return builder.Build();
	}
}
