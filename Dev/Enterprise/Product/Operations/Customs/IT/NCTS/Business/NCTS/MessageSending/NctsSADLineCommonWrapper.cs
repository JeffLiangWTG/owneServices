using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public abstract class NctsSADLineCommonWrapper : IETLine
{
	protected NctsSADLineCommonWrapper(NctsDepartureCargoDesc goodsItem)
	{
		GoodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		Header = Argument.NotNull(goodsItem.Header, nameof(goodsItem.Header));
	}

	public ZString DeclarationType => DeclarationTypeCore;
	protected abstract ZString DeclarationTypeCore { get; }

	public ITrader Consignor => new SADTraderWrapper(GoodsItem.Consignor);

	public ITrader Consignee => new SADTraderWrapper(GoodsItem.Consignee);

	public ZString DispatchCountryCode => DispatchCountryCodeCore;
	protected abstract ZString DispatchCountryCodeCore { get; }

	public ZString DestinationCountryCode => GoodsItem.BY_RN_NKCountryOfDestination;

	public IETLineSecurityBlock SecurityBlock => Header.BH_FTZMove ? new NctsSADLineSecurityBlockWrapper(GoodsItem) : new NctsSADLineEmptySecurityBlockWrapper();

	public IEnumerable<IPackage> Packages => GoodsItem.Packages.Any() ? new[] { new SADLinePackageWrapper(GoodsItem) } : Enumerable.Empty<SADLinePackageWrapper>();

	public IETLineSpecialMentionGroup SpecialMentionGroup => SpecialMentionGroupCore;
	protected abstract IETLineSpecialMentionGroup SpecialMentionGroupCore { get; }

	public ZString ComplementOfInformation => ZString.Empty;

	public ZString ComplementOfInformationLng => ZString.Empty;

	public ZInt ItemNumber => GoodsItem.BY_LineNo;

	public ZString CombinedNomenclature => GoodsItem.BY_HarmonisedTariff.Left(SADConstants.CustomsFieldMaxLength.EntryLine.CombinedNomenclatureCodeLength);

	public ZString GoodsDescription => GoodsItem.BY_Description;

	public IEnumerable<ZString> Containers => GoodsItem.ContainersSelected;

	public IEnumerable<ZString> AdditionalCodes => GoodsItem.AdditionalSupplementaryCodes.GetAllCodes().Distinct();

	public ZString CountryOfOrigin => GoodsItem.BY_RW_NKOriginState;

	public ZDecimal GrossMass => GoodsItem.GrossMassInKilograms;

	public ZString Procedure => GoodsItem.BY_Procedure;

	public IEnumerable<ZString> NationalProcedures => Enumerable.Empty<ZString>();

	public ZDecimal? NetMass => GoodsItem.NetMassInKilograms.GetValueOrNullIfZero();

	public IPreviousDocument PreviousAdministrativeDocument
	{
		get
		{
			var previousDocuments = GoodsItem.PreviousDocuments.Cast<NctsPreviousDocument>();
			if (!previousDocuments.Any())
			{
				return new SADEmptyPreviousDocumentWrapper();
			}
			else if (!previousDocuments.Skip(1).Any())
			{
				return new SADPreviousDocumentWrapper(previousDocuments.Single());
			}
			return new SADPreviousDocumentM2IndicatorWrapper();
		}
	}

	public ZDecimal? SupplementaryUnit => GoodsItem.BY_CustomsSecondQuantity > 0 ? GoodsItem.BY_CustomsSecondQuantity : null;

	public IEnumerable<ICertificate> Certificates => GoodsItem.SupportingDocuments.Cast<NctsSupportingDocument>().Select(doc => new SADCertificateWrapper(doc));

	public ZString Notes => SADWrapperHelper.RemoveBlackListChars(GoodsItem.Remarks);

	public IEnumerable<IDutyTaxFee> Duties => EntryLineFeesIncludedInMessageSending.Select(lineFee => new SADDutyTaxFeeWrapper(lineFee)).OrderBy(fee => fee.Type);

	IEnumerable<NctsCargoDescFee> EntryLineFeesIncludedInMessageSending => entryLineFeesIncludedInMessageSending ?? (entryLineFeesIncludedInMessageSending = GoodsItem.Fees.IncludedInMessageSending());
	IEnumerable<NctsCargoDescFee> entryLineFeesIncludedInMessageSending;

	public ZDecimal? TotalItemTaxedAmount => EntryLineFeesIncludedInMessageSending.GetTotalTaxedAmount();

	public ZDecimal? GrandTotalTaxedAmount => CalculateGrandTotalTaxedAmount();

	public ZDecimal? StatisticalValueAmount => null;

	#region Implementation

	protected NctsDepartureCargoDesc GoodsItem { get; }
	protected NctsHeader Header { get; }

	protected NctsDepartureMovementHeader DepartureMovement => Header.MovementHeader;

	ZDecimal? CalculateGrandTotalTaxedAmount()
	{
		var goodsItems = DepartureMovement.GoodsItems;

		var amount = goodsItems.IsLastGoodsItem(GoodsItem.BY_LineNo)
			? goodsItems.Cast<NctsDepartureCargoDesc>().Sum(x => x.Fees.IncludedInMessageSending().GetTotalTaxedAmount().Round(2))
			: (ZDecimal?)null;

		return amount;
	}

	#endregion
}
