using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class ExportOrientedUnitsLayoutBuilder : ColumnLayoutBuilder<JobDeclaration, ExportOrientedUnitsControlBag>
{
	public override ExportOrientedUnitsControlBag CommonBag => ExportOrientedUnitsControlBag.Instance;

	protected override int MaxColumns => 1;
}
