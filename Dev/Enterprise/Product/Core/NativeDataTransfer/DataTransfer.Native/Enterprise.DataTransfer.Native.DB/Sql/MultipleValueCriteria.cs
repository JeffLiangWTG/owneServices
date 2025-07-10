using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class MultipleValueCriteria
	{
		public string TableName { get; set; }
		public string ColumnName { get; set; }
		public virtual IEnumerable<object> Values { get; set; }

		/// <summary>
		/// Splits a MultipleValueCriteria into multiple new MultipleValueCriterias based on the number of Values.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<MultipleValueCriteria> SplitMyself(int numberOfValues)
		{
			var sourceValuesArray = Values.ToArray();

			if (sourceValuesArray.Length <= numberOfValues || numberOfValues <= 0)
			{
				yield return this;
			}
			else
			{
				var count = sourceValuesArray.Length / numberOfValues;
				var mod = sourceValuesArray.Length % numberOfValues;
				if (mod > 0)
				{
					count++;
				}

				for (int i = 0; i < count; ++i)
				{
					var length = (i == count - 1 && mod > 0) ? mod : numberOfValues;
					var destinationValueArray = new object[length];
					Array.Copy(sourceValuesArray, i * numberOfValues, destinationValueArray, 0, length);
					yield return new MultipleValueCriteria { TableName = this.TableName, ColumnName = this.ColumnName, Values = destinationValueArray.AsEnumerable() };
				}
			}
		}
	}
}