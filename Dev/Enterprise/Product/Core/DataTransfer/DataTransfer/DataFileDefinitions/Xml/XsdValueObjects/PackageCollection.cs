using System.Xml.Serialization;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class PackageCollection : Xsd.AutoPackageCollection
	{
		public void Add(PackageCollection packages)
		{
			foreach (Xsd.Package package in packages)
			{
				Add(package);
			}
		}

		/// <summary>
		/// Returns the total weight in KG of all packages in this collection rounded to the specified number of decimal places.
		/// </summary>
		/// <param name="RoundingScale"></param>
		/// <returns></returns>
		public decimal GetTotalWeightInKG(int decimalPlaces)
		{
			decimal totalWeightInKG = 0m;
			foreach (Xsd.Package package in this)
			{
				if (package.Weight.IsSpecified)
				{
					totalWeightInKG += Constants.Weight.Convert(package.Weight.Value, package.Weight.DimensionType, Constants.Weight.Kilograms);
				}
			}
			return Utilities.Round(totalWeightInKG, decimalPlaces);
		}

		/// <summary>
		/// Returns the total volume of all packages in this collection, rounded to the specified number of decimal places.
		/// </summary>
		public decimal GetTotalVolumeInM3(int decimalPlaces)
		{
			decimal totalCubicMetres = 0;
			foreach (Xsd.Package package in this)
			{
				if (package.Volume.IsSpecified)
				{
					totalCubicMetres += Constants.Volume.Convert(package.Volume.Value, package.Volume.DimensionType, Constants.Volume.CubicMetres);
				}
			}
			return Utilities.Round(totalCubicMetres, decimalPlaces);
		}

		public int TotalNumberOfPacks
		{
			get
			{
				int total = 0;

				foreach (Xsd.Package package in this)
				{
					total += (int)package.NumberOfPacks;
				}

				return total;
			}
		}
	}
}
