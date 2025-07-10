using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EES
{
	public sealed class EESDataRegistry : RegistryItemSet
	{
		#region Instance

		public static EESDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new EESDataRegistry();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static EESDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		const string Category = "EES Client Extensions";
		const string DocumentsCategory = Category + "/Documents";

		#region NumberOfOriginalBillsToBePrintedOnDotMatrix
		public ZInt NumberOfOriginalBillsToBePrintedOnDotMatrix
		{
			get
			{
				return new ZInt(NumberOfOriginalBillsToBePrintedOnDotMatrixItem.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
			set
			{
				NumberOfOriginalBillsToBePrintedOnDotMatrixItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, (int)value);
			}
		}

		IntRegistryItem NumberOfOriginalBillsToBePrintedOnDotMatrixItem
		{
			get
			{
				return GetItem("NumberOfOriginalBillsToBePrintedOnDotMatrix", delegate
				{
					return new IntRegistryItem("NumberOfOriginalBillsToBePrintedOnDotMatrix", (NoResString)DocumentsCategory, (NoResString)"No. of Originals for dot-matrix House Bills", (NoResString)"Enter the number of Originals to print", RegistryStorageFlags.Company, RegistryOptions.Default, 1);
				});
			}
		}
		#endregion
	}
}
