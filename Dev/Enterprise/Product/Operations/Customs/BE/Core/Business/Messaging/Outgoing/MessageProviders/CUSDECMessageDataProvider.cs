using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using BECusEntryHeader = Enterprise.Customs.BE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.BE.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used In CUSDECMessageManager")]
public class CUSDECMessageDataProvider : ICUSDECMessageDataProvider
{
	#region Implementation
	public CUSDECMessageDataProvider(BECusEntryHeader entry)
	{
		GoodsDeclaration = new GoodsDeclaration(entry);
		GoodsItem = entry.MergedLines.Select(cl => new GoodsItem(cl)).OrderBy(gi => gi.Sequence).ToArray();
	}
	#endregion
	public ZString FunctionCode { get => "09"; }
	public ITGoodsDeclarationImport GoodsDeclaration { get; set; }
	public ITGoodsItemImport[] GoodsItem { get; set; }
	public ZString LanguageCode { get => GlbStaff.CurrentUser.Language; }
	public ITOperator MessageSender { get => GoodsDeclaration.Declarant; }
}
