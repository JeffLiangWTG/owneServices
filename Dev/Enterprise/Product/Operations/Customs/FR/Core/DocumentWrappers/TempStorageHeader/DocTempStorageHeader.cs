using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.DocumentWrappers.TempStorageHeader;

public class DocTempStorageHeader : DocBaseWrapper
{
	DocTempStorageHeader(CusTempStorageJobHeader header, BusinessObjectFactory factory) : base(header, factory)
	{
		Argument.NotNull(header, "header");
	}

	public static DocTempStorageHeader New(CusTempStorageJobHeader header, BusinessObjectFactory factoryToWrap)
	{
		return header == null ? null : new DocTempStorageHeader(header, factoryToWrap);
	}

	#region Related Business Objects

	CusTempStorageJobHeader Header => (CusTempStorageJobHeader)WrappedObject;

	public DocTempStorageLineCollection Lines
	{
		get
		{
			var result = new DocTempStorageLineCollection(Factory);
			if (Header.CusTempStorageDec != null)
			{
				result.AddRange(Header.CusTempStorageDec.CusTempStorageLines.Cast<CusTempStorageLine>().Select(x => DocTempStorageLine.New(x, Factory)));
			}
			return result;
		}
	}

	#endregion

	#region Properties

	public ZString InternalReference => Header.SJH_JobReference;

	public ZString CustomerReference => Header.DDTNumber + " / " + Header.SJH_ReferenceNumber;

	public ZString ReportDate => Header.SJH_PresentationDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

	public ZString CustomerAddress => Header.Customer?.MainAddress?.AddressFullFormatted ?? ZString.Empty;

	public ZString CustomerTelephone => Header.Customer?.MainAddress?.OA_Phone ?? ZString.Empty;

	public ZString CustomerAgreement => Header.SJH_CustomsProfile;

	public ZString TaxID => Header.Customer?.CustomsCodes.Cast<OrgCusCode>()
							.FirstOrDefault(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.France && x.OK_CodeType == OrgCusCode.FranceCodeTypes.Siret)?.OK_CustomsRegNo
							?? ZString.Empty;

	public ZString OfficeCode => Header.SJH_CustomsOffice;

	public ZString OfficeName => Header.SJH_CustomsOfficeDescription;

	public ZString PreviousDocument => FormattableString.Invariant($"{Header.SJH_PreviousReferenceType} Z {Header.SJH_PreviousReferenceNumber}");

	public ZString CountryOfOrigin => Header.SJH_RL_NKLoading.Left(2);

	public ZString ContainerNumber => Header.CusTempStorageDec?.STH_Calc_Containers ?? ZString.Empty;

	public ZDecimal TotalGrossWeight => Header.CusTempStorageDec?.CusTempStorageLines.Cast<CusTempStorageLine>().Sum(x => x.TSL_GrossWeight) ?? ZDecimal.Zero;

	#endregion
}
