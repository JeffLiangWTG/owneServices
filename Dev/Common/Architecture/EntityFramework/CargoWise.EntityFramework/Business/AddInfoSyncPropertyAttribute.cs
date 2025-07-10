using System;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class AddInfoSyncPropertyAttribute : Attribute
	{
		/// <summary>
		/// Specify that the property will be sync in XX_AddInfo field.
		/// <example>For example:
		/// <code>
		///   [AddInfoSyncProperty("K84AccountingDate", typeof(ZDateTime), "CA_K84AccountingDate")]
		///   public override ZDate CAD_K84AccountingDate
		///   ...
		///   ...
		///   CAD_K84AccountingDate = new ZDate(2021, 12, 15);
		/// </code>
		/// results in JE_AddInfo='K84AccountingDate=2021-12-15 00:00:00.000' and GenAddOnColumn (XA_Name='CA_K84AccountingDate', XA_Type='DAT', XA_Data='2021-12-15 00:00:00.000')
		/// </example>
		/// </summary>
		/// <param name="addInfoName">The add info key name</param>
		/// <param name="addInfoValueType">The add info value type</param>
		/// <param name="fastSearchName">The name to store in GenAddOnColumn.XA_Name. Specify this if we want to create GenAddOnColumn</param>
		public AddInfoSyncPropertyAttribute(string addInfoName, Type addInfoValueType, string fastSearchName = null)
		{
			this.AddInfoName = Argument.NotNullOrEmpty(addInfoName, nameof(addInfoName));
			this.AddInfoValueType = Argument.NotNull(addInfoValueType, nameof(addInfoValueType));
			if (fastSearchName != null && fastSearchName.Length == 0)
			{
				throw new ArgumentException("fastSearchName parameter should be either null or not empty");
			}
			this.FastSearchName = fastSearchName;
		}
		public readonly string AddInfoName;
		public readonly Type AddInfoValueType;
		public readonly string FastSearchName;
	}
}
