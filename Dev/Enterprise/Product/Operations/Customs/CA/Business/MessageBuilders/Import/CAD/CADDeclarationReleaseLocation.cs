using System.Collections.Generic;
using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationReleaseLocation : ICADMessageDeclarationReleaseLocation
{
	public CADDeclarationReleaseLocation(JobDeclaration declaration, bool isWarehouse)
	{
		this.declaration = declaration;
		this.isWarehouse = isWarehouse;
	}

	readonly JobDeclaration declaration;
	readonly bool isWarehouse;
	const string RoleCodeST = "ST";
	const string RoleCoseSF = "SF";
	const string TypeCode18 = "18";

	#region ICADMessageDeclarationReleaseLocation

	string ICADMessageDeclarationReleaseLocation.ID => declaration.JE_CustomsOffice;

	IEnumerable<ICADMessageDeclarationReleaseLocationWarehouse> ICADMessageDeclarationReleaseLocation.Warehouse
	{
		get
		{
			if (isWarehouse)
			{
				if (CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationST(declaration.JE_MessageSubType))
				{
					var cadDeclarationReleaseLocationWarehouse = GenerateCADDeclarationReleaseLocationWarehouse(declaration, RoleCodeST);
					if (cadDeclarationReleaseLocationWarehouse != null)
					{
						yield return cadDeclarationReleaseLocationWarehouse;
					}
				}

				var tempList = new List<ZString>();
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					foreach (var dutyAndTax in invoiceLine.DutiesAndTaxes)
					{
						var previousTranNumber = dutyAndTax.C1_PreviousTranNumber;
						if (!previousTranNumber.IsEmpty && !tempList.Contains(previousTranNumber))
						{
							tempList.Add(previousTranNumber);
							var entryNum = CusEntryNumber.LoadMostRecentByCreateTime(declaration.Factory, CusEntryNumber.EntryType.CATransactionNumber, previousTranNumber, Enterprise.Core.Constants.CountryCodes.Canada);
							if (entryNum?.Parent is JobDeclaration previousDeclaration && CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(previousDeclaration.JE_MessageSubType))
							{
								var cadDeclarationReleaseLocationWarehouse = GenerateCADDeclarationReleaseLocationWarehouse(previousDeclaration, RoleCoseSF);
								if (cadDeclarationReleaseLocationWarehouse != null)
								{
									yield return cadDeclarationReleaseLocationWarehouse;
								}
							}
						}
					}
				}
			}
		}
	}

	CADDeclarationReleaseLocationWarehouse GenerateCADDeclarationReleaseLocationWarehouse(JobDeclaration declaration, string roleCode)
	{
		var bondedWareHouseOrg = declaration.WarehouseDocAddress?.Organisation;
		if (bondedWareHouseOrg != null)
		{
			var cpw = bondedWareHouseOrg.CustomsCodes.GetCustomsRegNo(MasterFiles.Business.OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Core.Constants.CountryCodes.Canada);
			if (!cpw.IsEmpty)
			{
				return new CADDeclarationReleaseLocationWarehouse(id: cpw, typeCode: TypeCode18, roleCode: roleCode);
			}
		}
		return null;
	}

	#endregion
}
