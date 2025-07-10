using System;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.AU.Declaration.GUI;

public class AHECCTariffColumnStyleInfo : ZBaseFindBoxColumnStyleInfo
{
	public AHECCTariffColumnStyleInfo(IsImportDelegate isImportDelegate)
	{
		this.isImportDelegate = isImportDelegate;
	}
	readonly IsImportDelegate isImportDelegate;

	public override Type ColumnStyleType => typeof(AHECCTariffColumnStyle);

	protected internal class AHECCTariffColumnStyle : ZBaseFindBoxColumnStyle
	{
		public AHECCTariffColumnStyle(AHECCTariffColumnStyleInfo info)
			: base(() => new AHECCTariffGridFindBox(info.isImportDelegate), info)
		{
		}
	}
}
