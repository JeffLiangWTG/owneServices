using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.DataConverters.CustomsFiles.AU.DataImporters.DbCyber2
{
	public class PartDataImporter : InterbaseImporter
	{
		public PartDataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool excludeExistingRecords, ZBool importToCSVFile)
			: base(logger, dataSourcePath, excludeExistingRecords, importToCSVFile)
		{
		}

		protected override string DataTypeDescription
		{
			get { return "Parts"; }
		}

		protected override internal DataWriter GetNextDataWriter(BusinessObjectFactory factory)
		{
			DataRow partRecord = GetNextDataRow();

			PartWriter writer = new PartWriter(factory);
			writer.PartNumber = new ZString(partRecord[Cyber2Schema.Product.PartCode]).Trim();
			writer.LookupCode = new ZString(partRecord[Cyber2Schema.Product.Lookup]).Trim();
			writer.ImporterCode = new ZString(partRecord[Cyber2Schema.Product.Buyer]).Trim();
			writer.SupplierCode = new ZString(partRecord[Cyber2Schema.Product.Supplier]).Trim();
			writer.Description = new ZString(partRecord[Cyber2Schema.Product.Description]).Trim();
			writer.Weight = (partRecord[Cyber2Schema.Product.Weight] == DBNull.Value) ? 0.0m : Convert.ToDecimal(partRecord[Cyber2Schema.Product.Weight]);
			writer.Volume = (partRecord[Cyber2Schema.Product.Volume] == DBNull.Value) ? 0.0m : Convert.ToDecimal(partRecord[Cyber2Schema.Product.Volume]);
			writer.WeightUQ = Env.Registry.PackageWeightUnit;
			writer.VolumeUQ = Env.Registry.PackageVolumeUnit;

			if (partRecord[Cyber2Schema.Product.UQ] != null && new ZString(partRecord[Cyber2Schema.Product.UQ]).Trim() != "")
			{
				writer.DefaultStockUnit = new ZString(partRecord[Cyber2Schema.Product.UQ]).Trim();
			}
			else
			{
				writer.DefaultStockUnit = Env.Registry.DefaultStockUnit;
			}

			return writer;
		}

		protected override internal ZString SqlText
		{
			get { return new ZString("select " + Fields + " from CustPartRecord"); }
		}

		protected string Fields
		{
			get { return "AccountId, Supplier, PartNo, Description, UQ, UQ1, Lookup, P_group, Weight, Volume"; }
		}

		protected override internal ZString CSVOutputHeader
		{
			get { return "Code,Description,UQ,TariffLookup,AltTariffLookup,Owner,Supplier,Division,QtyInStock"; }
		}
	}
}
