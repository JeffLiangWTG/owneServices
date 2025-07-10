using System.ComponentModel;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class GoodsLocationLBLCUserControl : ZUserControl
{
	public GoodsLocationLBLCUserControl()
	{
		InitializeComponent();

		SubLocationOfGoodsAsCountryFindBox.MaxLength = JobDeclaration.Schema.ImportJE_SubLocationOfGoodsMaxLength;
		LocationOtherInformationTextBox.MaxLength = JobDeclaration.Schema.ImportJE_LocationOtherInformationMaxLength;

#if DEBUG
		TypeDescriptor.AddAttributes(LocationQualifierDropEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(SubLocationOfGoodsAsCountryFindBox, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(GoodsLocationDropEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(LocationOtherInformationTextBox, new SuppressControlRequiresTextBasherAttribute());
#endif
	}
}
