using System.ComponentModel;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class GoodsLocationDUserControl : ZUserControl
{
	public GoodsLocationDUserControl()
	{
		InitializeComponent();

		LocationOfGoodsAsCustomsOfficeFindBox.MaxLength = JobDeclaration.Schema.JE_LocationOfGoodsAsCustomsOfficeMaxLength;

#if DEBUG
		TypeDescriptor.AddAttributes(LocationQualifierDropEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(LocationOfGoodsAsCustomsOfficeFindBox, new SuppressControlRequiresTextBasherAttribute());
#endif
	}
}
