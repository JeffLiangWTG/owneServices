using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsManifestLineSequence : CusCodeData, Integration.Customs.AU.ICustomsManifestLineSequence
	{
		public CustomsManifestLineSequence(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SuspendValidation();
		}

		public static class DataCodes
		{
			public const string DeletedLineNumber = "DLN";
			public const string PreliminaryDeletedLine = "PDL";
			public const string PreliminaryLineNumber = "PLN";
			public const string ManifestLineNumber = "MLN";
		}

		#region Parent

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CommonShipment), ObjectFactory.GetType<IHVLVConsignment>()); }
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CustomsManifestLineSequence;
		}

		public bool IsPreliminaryNumberType
		{
			get { return CY_Code == DataCodes.PreliminaryLineNumber; }
		}

		public void UpdatePreliminaryLineNumberToManifestedLineNumber()
		{
			CY_Code = DataCodes.ManifestLineNumber;
		}

		public void UpdateManifestedLineNumberToPreliminaryDeletedLine()
		{
			CY_Code = DataCodes.PreliminaryDeletedLine;
		}

		public void ResetPreliminaryDeleteLineToManifested()
		{
			CY_Code = DataCodes.ManifestLineNumber;
		}

		public bool IsPreliminaryDeletedLine
		{
			get { return CY_Code == DataCodes.PreliminaryDeletedLine; }
		}

		public void UpdatePreliminaryDeletedLineNumberToDeletedLine()
		{
			CY_Code = DataCodes.DeletedLineNumber;
		}
	}
}
