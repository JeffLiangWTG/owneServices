using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// A barcoded DocumentResult. Will be converted to a storagedocs record once it has been
	/// processed by the business layer.
	/// </summary>
	public class BaseBarcode : DocumentResult
	{
		public BaseBarcode(DocumentFactory masterFactory)
			: this(masterFactory, null)
		{
		}

		public BaseBarcode(DocumentFactory masterFactory, ZString barcodeText)
			: base(masterFactory)
		{
			fFullBarcodeText = barcodeText;
		}

		#region Full Barcode Text

		public ZString FullBarcodeText
		{
			get { return fFullBarcodeText; }
		}
		readonly ZString fFullBarcodeText;

		#endregion

		#region DocManagerCode

		public ZString DocManagerCode
		{
			get { return fDocManagerCode; }
			set
			{
				fDocManagerCode = value;
				GetPKFromCode();
			}
		}
		protected ZString fDocManagerCode;

		#endregion

		#region RefCode e.g. S00001010

		public ZString RefCode
		{
			get { return fRefCode; }
			set
			{
				fRefCode = value;
				GetPKFromCode();
			}
		}
		protected ZString fRefCode;

		#endregion

		#region RefPK

		public virtual ZGuid RefPK
		{
			get { return fRefPK; }
			set { fRefPK = value; }
		}
		protected ZGuid fRefPK;

		#endregion

		#region DocType

		public ZString DocType
		{
			get { return fDocType; }
			set { fDocType = value; }
		}
		protected ZString fDocType;

		#endregion

		#region CompanyCode

		public virtual ZString CompanyCode
		{
			get { return fCompanyCode; }
			set
			{
				fCompanyCode = value;
				GetPKFromCode();
			}
		}
		protected ZString fCompanyCode;

		#endregion

		#region IsRefTypeEncoded
		public bool ContainsRefTypeData
		{
			get { return BarcodeHelper.IsRefTypeEncoded(FullBarcodeText); }
		}
		#endregion

		#region Implementation

		void GetPKFromCode()
		{
			if (!DocManagerCode.IsEmpty && !RefCode.IsEmpty && AssemblyDataLookup.IsDocManagerCodeValid(DocManagerCode))
			{
				RefPK = CompanyCode.IsEmpty ? AssemblyDataLookup.GetPKFromCode(MasterFactory, DocManagerCode, RefCode, false) : AssemblyDataLookup.GetPKFromCode(MasterFactory, DocManagerCode, RefCode, CompanyCode, false);
			}
		}

		#endregion
	}
}
