using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class CusBondDetailValueObjectDataAdapter : ValueObjectDataAdapter<CusBondDetail, Xsd.BondDetail>
	{
		public override string RootCollectionElementName
		{
			get { return "BondDetails"; }
		}

		public override string RootElementName
		{
			get { return "BondDetail"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleBondDetailSchema; }
		}

		#region Import

		public void ImportAllBondDetails(OrgHeader organisation, Xsd.BondDetailCollection bondDetails, BusinessObjectFactory factory, IValueObjectImportContext context)
		{
			if (bondDetails.Count > 0)
			{
				CusBondDetailCollection bondCollection = new CusBondDetailCollection(organisation);
				bondCollection.Load();
				if (bondCollection.Count > 0)
				{
					bondCollection.RemoveAndDeleteAll();
				}

				foreach (Xsd.BondDetail xsdBond in bondDetails)
				{
					CusBondDetail bond = bondCollection.AddNew();
					ImportFromValueObject(bond, xsdBond, context);
				}
			}
		}

		protected override void ImportFromValueObjectCore(CusBondDetail bizObj, Xsd.BondDetail valueObj, IValueObjectImportContext context)
		{
			ImportBondDetails(bizObj, valueObj, context);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML Mapping Name")]
		void ImportBondDetails(CusBondDetail bond, Xsd.BondDetail xsdBond, IValueObjectImportContext context)
		{
			if (xsdBond.ActivityCodeSpecified)
			{
				context.SetPropertyInfoValue(bond.PW_ActivityCodeInfo, BondActivityCodeXmlMappings.Instance.GetEnterpriseCode(xsdBond.ActivityCode.ToString(), "", context), true, "Bond Activity Type");
			}

			if (xsdBond.AmountSpecified)
			{
				bond.PW_BondAmount = xsdBond.Amount;
			}
			bond.PW_BondEffectiveDate = xsdBond.Effective;
			bond.PW_BondExpiryDate = xsdBond.Expiry;
			bond.PW_BondFiledPort = xsdBond.FiledPort.Left(bond.PW_BondFiledPortInfo.MaxLength);
			bond.PW_BondNumber = xsdBond.Number.Left(bond.PW_BondNumberInfo.MaxLength);
			bond.PW_SuretyCode = xsdBond.SuretyCode.Left(bond.PW_SuretyCodeInfo.MaxLength);
			bond.PW_BondType = xsdBond.Type.Left(bond.PW_BondTypeInfo.MaxLength);
		}

		#endregion

		#region Export

		public void ExportAllBondDetails(ZGuid organisationPK, Xsd.BondDetailCollection bondDetails, BusinessObjectFactory factory, IValueObjectExportContext context)
		{
			ZQuery query = new ZQuery(CusBondDetailSchema.PW_ParentID, organisationPK);
			CusBondDetail[] loadedBonds = factory.Load<CusBondDetail>(query);

			foreach (CusBondDetail bond in loadedBonds)
			{
				Xsd.BondDetail xsdBond = new Xsd.BondDetail();
				ExportToValueObject(bond, xsdBond, context);
				bondDetails.Add(xsdBond);
			}
		}

		protected override void ExportToValueObjectCore(CusBondDetail bizObj, Enterprise.DataTransfer.Xml.XsdVersion1.BondDetail constructedValueObject, IValueObjectExportContext context)
		{
			ExportBondDetails(bizObj, constructedValueObject, context);
		}

		void ExportBondDetails(CusBondDetail bond, Xsd.BondDetail xsdBond, IValueObjectExportContext context)
		{
			if (!bond.PW_ActivityCode.IsEmpty)
			{
				xsdBond.ActivityCode = BondActivityCodeXmlMappings.Instance.GetExternalCode(bond.PW_ActivityCode, "", context);
				xsdBond.ActivityCodeSpecified = true;
			}

			if (bond.PW_BondAmount > 0)
			{
				xsdBond.Amount = bond.PW_BondAmount;
				xsdBond.AmountSpecified = true;
			}
			xsdBond.Effective = bond.PW_BondEffectiveDate.Date;
			xsdBond.Expiry = bond.PW_BondExpiryDate.Date;
			xsdBond.FiledPort = bond.PW_BondFiledPort;
			xsdBond.Number = bond.PW_BondNumber;
			xsdBond.SuretyCode = bond.PW_SuretyCode;
			xsdBond.Type = bond.PW_BondType;
			xsdBond.IsSpecified = true;
		}

		#endregion
	}
}
