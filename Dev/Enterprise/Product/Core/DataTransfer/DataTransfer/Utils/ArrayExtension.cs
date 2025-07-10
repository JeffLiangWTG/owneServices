using System;
using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Business
{
	public static class ArrayExtension
	{
		public static BusinessObject[] OfType(this BusinessObject[] elements, Type targetElementType)
		{
			var result = (BusinessObject[])Array.CreateInstance(targetElementType, elements.Length);

			if (elements.Length <= 0)
			{
				return result;
			}

			for (int i = 0; i < elements.Length; i++)
			{
				result[i] = elements[0].Factory.Load(targetElementType, elements[i].PK);
			}
			return result;
		}
	}
}
