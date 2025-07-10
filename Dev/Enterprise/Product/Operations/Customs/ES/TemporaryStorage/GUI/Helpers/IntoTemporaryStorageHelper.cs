using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public static class IntoTemporaryStorageHelper
{
	public static bool GoodsLocationExistsInPremises(BusinessObjectFactory factory, ZString goodsLocation, ZBool isMessageTypeLAM)
	{
		var premisesType = isMessageTypeLAM ? CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility : CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		var premises = (CusTempStorageRegPremises)(TemporaryStorageHelper.GetManagedPremises(factory, premisesType, goodsLocation));
		var result = true;

		if (premises == null)
		{
			result = false;
			var error = ResString.GetMultilingualString("0C3B4FF6-2A12-4CBB-8820-A9547A4660F1",
														"Temporary Storage Location ({0}) does not exist in Maintain/Customs/Customs Files/Temporary Storage Premises module.",
														goodsLocation);
			Globals.Message.ShowError(error);
		}
		else
		{
			var activeWrapper = premises.NumberProvider.ActiveWrapper;
			if (activeWrapper == null || activeWrapper.SN_AvailableNumbers.IsEmpty)
			{
				result = false;
				var error = ResString.GetMultilingualString("62257D2D-991A-4C61-BBF6-5BE9E77A181D",
															"The associated Premises to the Temporary Storage Location ({0}) has no active numbering configuration or it has no available numbers.",
															goodsLocation);
				Globals.Message.ShowError(error);
			}
		}

		return result;
	}

	public static string GetMutexLockText(string user)
		=> Res.GetString("5E809990-0EE7-49C5-B79A-5670FC64D362",
			"{0} is already in the process of creating a Temporary Storage Register Header.\r\nYou should be able to access this option when the person has saved the record. Please try later.", user);

	public static bool GuaranteeAndLiabilityAmountAreDeclared(CommonGuarantee guarantee)
	{
		var guaranteeIsDeclaredCorrectly = guarantee != null && !guarantee.PW_BondNumber.IsEmpty;

		if (!guaranteeIsDeclaredCorrectly)
		{
			var error = ResString.GetMultilingualString("4C2CAC63-A33D-4045-A9E3-81164DD39B49",
														"To enter goods into the Temporary Storage, a liability amount for a related Guarantee must be supplied.");
			Globals.Message.ShowError(error);
		}

		return guaranteeIsDeclaredCorrectly;
	}

	public static bool ShowWarningIfGuaranteeLiabilityAmountIsZero(CommonGuarantee guarantee)
	{
		var guaranteeIsDeclaredCorrectly = guarantee != null && !guarantee.PW_BondAmount.IsEmpty;

		if (!guaranteeIsDeclaredCorrectly)
		{
			var error = ResString.GetMultilingualString("875C73DE-9744-4205-A134-AE58A0AA2215",
@"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)");

			var result = Globals.Message.Show(
				error,
				Res.GetString("AC8360BE-9F69-406F-A069-621E812156BD", "Into Temporary Storage"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.No;

			return result;
		}

		return guaranteeIsDeclaredCorrectly;
	}

	public static bool GuaranteNumberExistsAndIsValid(CommonGuarantee guarantee, CusGuaranteeHeader cusPermitHeader)
	{
		var existsAndIsValid = false;
		var bondNumber = guarantee?.PW_BondNumber ?? ZString.Empty;

		if (!bondNumber.IsEmpty)
		{
			var todayDate = ZDate.Today;
			if (cusPermitHeader != null &&
				cusPermitHeader.CPH_StartDate < todayDate &&
				(cusPermitHeader.CPH_EndDate.IsEmpty || cusPermitHeader.CPH_EndDate > todayDate) &&
				cusPermitHeader.HasOpeningBalanceTransaction)
			{
				existsAndIsValid = true;
			}
		}

		if (!existsAndIsValid)
		{
			var error = ResString.GetMultilingualString("05283B80-34F6-4E34-ABF0-A2ECAFA5067F",
														"Guarantee Nº ({0}) does not exist in Maintain/Customs/Customs Files/Customs Guarantees module or is not valid.",
														bondNumber);
			Globals.Message.ShowError(error);
		}

		return existsAndIsValid;
	}

	public static void ShowTemporaryStorageRegisterForm(BusinessObjectFactory factory, ZString summaryEntryNum, ZString goodsLocation)
	{
		var query = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));
		_ = query.AddToFilter(CusTempStorageRegHeaderSchema.SRH_Reference, summaryEntryNum);
		var premisesSubQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegPremises), CusTempStorageRegPremisesSchema.PK);
		_ = premisesSubQuery.AddToFilter(CusTempStorageRegPremisesSchema.SRP_CustomsLocation, goodsLocation);
		query.AddSubQuery(CusTempStorageRegHeaderSchema.SRH_SRP_Premises, premisesSubQuery, JoinCondition.And);

		var header = factory.LoadTop1<ES.Business.CusTempStorage.CusTempStorageRegHeader>(query);

		if (header != null)
		{
			var temporaryStorageRegisterForm = new TempStorageRegisterForm(header);
			ZFormModaliser.ShowDialogAndDispose(temporaryStorageRegisterForm);
		}
		else
		{
			var error = ResString.GetMultilingualString("A8577A9F-7EC4-4E11-BCD7-684B82556E6C",
														"No Entry in the Temporary Register found for ({0}) in ({1})",
														summaryEntryNum, goodsLocation);
			Globals.Message.ShowError(error);
		}
	}

	public static bool GoodsItemsAreNotInTemporaryStorage(BusinessObjectFactory factory, ZString summaryEntryNum, ZBool isMessageTypeLAM)
	{
		var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_AppCode, ES.Business.CusTempStorage.CusTempStorageRegHeader.ESAppCode);
		_ = query.AddToFilter(CusTempStorageRegHeaderSchema.SRH_Reference, summaryEntryNum);

		var isInTempStorage = factory.Exists(typeof(CusTempStorageRegHeader), query);

		if (isInTempStorage)
		{
			var error = isMessageTypeLAM
						? ResString.GetMultilingualString("74993A98-D568-4F37-9880-0013E6CF894A",
													"Goods Items for LAME Reception Certificate {0} are already in the Temporary Storage.",
													summaryEntryNum)
						: ResString.GetMultilingualString("251BF657-0E2C-4F59-9F5B-16C56E4B2B84",
													"Goods Items from Summary Declaration {0} are already in the Temporary Storage.",
													summaryEntryNum);
			Globals.Message.ShowError(error);
		}

		return !isInTempStorage;
	}

	public static bool AtLeastOneLineNotMissing(TemporaryStorageHeader header)
	{
		var anyLineNotMISWithPackages = header.Bills.Any(bill =>
		{
			return bill.PackedItems.Cast<ES.Business.CusTempStorage.TemporaryStoragePackedItem>().Any(line => !line.IsMissing && line.PackagesPivot.Count > 0);
		});

		if (!anyLineNotMISWithPackages)
		{
			var error = ResString.GetMultilingualString("D016BD16-F993-47B1-908B-5160DC43894B",
														"There are no good items available to enter the Temporary Storage.");
			Globals.Message.ShowError(error);
		}

		return anyLineNotMISWithPackages;
	}

	public static void ShowWarningIfMoreThanOnePackageLinkedToALine(TemporaryStorageHeader header)
	{
		var anyLineHasMoreThanOnePack = header.Bills.Any(bill =>
		{
			return bill.PackedItems.Cast<ES.Business.CusTempStorage.TemporaryStoragePackedItem>().Any(line => !line.IsMissing && line.PackagesPivot.Count > 1);
		});

		if (anyLineHasMoreThanOnePack)
		{
			var warning = ResString.GetMultilingualString("153AB886-39E6-4C26-963A-B4B25DE1E05D",
														"There are some Items with multiple package lines. For these packages, the Goods Item’s gross weight will be automatically apportioned on the Temporary Storage Register.");
			Globals.Message.ShowWarning(warning);
		}
	}

	public static bool OnlyOneLineForLAM(TemporaryStorageHeader header)
	{
		var onlyOneLine = header.Bills.SelectMany(bill => bill.PackedItems.Cast<TemporaryStoragePackedItem>()).Count() == 1;

		if (!onlyOneLine)
		{
			var error = ResString.GetMultilingualString("EF43E7FA-67D7-4D76-8D26-4617E72F8B20",
														"Only one Goods Item is allowed per LAME Reception Certificate. Please, create a different LAM record per Goods Item and try again.");
			Globals.Message.ShowError(error);
		}

		return onlyOneLine;
	}

	public static string GetFormattedManualLocationOfGoodsDescription(string code) => $"{CusGoodsLocationQualifierList.Codes.AuthorizationNumber};{CusGoodsLocationTypeList.Codes.AuthorizedPlace};{code}";
}
