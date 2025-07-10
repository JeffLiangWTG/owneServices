using System.ComponentModel;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class GoodsLocationFCUserControl : ZUserControl
{
	public GoodsLocationFCUserControl()
	{
		InitializeComponent();

		SubLocationOfGoodsAsCountryFindBox.MaxLength = JobDeclaration.Schema.ImportJE_SubLocationOfGoodsMaxLength;
		LocationOfGoodsTextBox.MaxLength = JobDeclaration.Schema.JE_LocationOfGoodsAsCodeMaxLength;

#if DEBUG
		TypeDescriptor.AddAttributes(LocationQualifierDropEdit, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(SubLocationOfGoodsAsCountryFindBox, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(LocationOfGoodsTextBox, new SuppressControlRequiresTextBasherAttribute());
#endif
	}
}
