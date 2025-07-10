using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class GoodsLocationImportExport : ITGoodsLocationImportExport
{
	public GoodsLocationImportExport(JobDeclaration declaration)
	{
		Argument.NotNull(declaration, JobDeclaration.Schema.TableName);
		this.declaration = declaration;
	}

	readonly JobDeclaration declaration;

	public ZString AgreedLocation { get => ZString.Empty; }
	public ZString AuthorisedLocation { get => ZString.Empty; }
	public ZString Precise { get => declaration.JE_LocationOfGoods.IsEmpty ? declaration.JE_CustomsOffice : declaration.JE_LocationOfGoods; }
}
