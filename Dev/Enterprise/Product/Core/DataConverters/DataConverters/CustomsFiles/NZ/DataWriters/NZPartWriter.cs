using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataConverters.CustomsFiles.NZ
{
	public sealed class PartWriter : CustomsFiles.PartWriter
	{
		public PartWriter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Public Fields to be filled in when importing.
		public ZDecimal UnitQuantityFactor;
		#endregion

		/// <summary>
		/// New Zealand specific import fields.
		/// </summary>
		/// <param name="part"></param>
		protected override void UpdateCountrySpecificData(BusinessObject part)
		{
		}

		protected override Type GetPartType()
		{
			return ObjectFactory.GetType<Integration.Customs.NZ.IOrgSupplierPart>();
		}
	}
}
