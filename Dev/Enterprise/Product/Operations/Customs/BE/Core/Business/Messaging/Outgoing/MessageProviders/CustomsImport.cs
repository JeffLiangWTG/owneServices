using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class CustomsImport : ITCustomsImport
{
	public CustomsImport(JobDeclaration declaration)
	{
		Argument.NotNull(declaration, JobDeclaration.Schema.TableName);
		this.declaration = declaration;
		GoodsLocation = new GoodsLocationImportExport(declaration);
	}

	readonly JobDeclaration declaration;

	public ITGoodsLocationImportExport GoodsLocation { get; set; }
	public ZString PresentationCustomsOffice { get => ZString.Empty; }
	public ITSeal[] Seal { get; set; }
	public ZString Seals { get => "0"; }
	public ZString SupervisingCustomsOffice { get => ZString.Empty; }
	public ZString ValidationOffice { get => declaration.JE_CustomsOffice; }
}
