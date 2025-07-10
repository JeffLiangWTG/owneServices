using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPAMDLineProvider : AESLineProvider, IEXPAMDLine
	{
		public EXPAMDLineProvider(CusEntryLine entryLine, IEXPAMDHeader headerProvider) : base(entryLine)
		{
			this.headerProvider = Argument.NotNull(headerProvider, nameof(headerProvider));
		}
		readonly IEXPAMDHeader headerProvider;

		public ZString CommercialReferenceNumber => headerProvider.CommercialReferenceNumber.IsNullOrEmpty() ? RandomInvoiceHeader.JZ_UCR : ZString.Empty;

		public bool CommoditySpecified => EntryHeader.EntryInstruction.Style4thDigitIs2();

		public override bool StatisticalValueSpecified => base.StatisticalValueSpecified && CommoditySpecified;

		public ZDecimal GrossMass => CachedValueHelper.GetValue(ref grossMass, () => EntryLine.EffectiveGrossWeight.InKilogramsSafe.Round(3).Normalize());
		CachedValue<ZDecimal> grossMass;

		public ZDecimal NetMass => CachedValueHelper.GetValue(ref netMass, () => EntryLine.EffectiveCustomsWeight.Amount.Round(3).Normalize());
		CachedValue<ZDecimal> netMass;

		public IReadOnlyCollection<IPackage> Packages => packages ?? (packages = EntryLine.PackagingDetails.Select(LinePackageProvider.NewOrNull).ToArray());
		IReadOnlyCollection<IPackage> packages;
	}
}
