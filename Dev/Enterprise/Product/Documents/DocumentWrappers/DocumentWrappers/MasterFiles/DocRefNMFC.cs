using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocRefNMFC : DocumentWrapper
	{
		DocRefNMFC(RefNMFC nMFCArticle, BusinessObjectFactory factoryToWrap)
			: base(nMFCArticle, factoryToWrap)
		{
		}

		public static DocRefNMFC New(RefNMFC nMFCArticle, BusinessObjectFactory factoryToWrap)
		{
			if (nMFCArticle == null)
			{
				return null;
			}
			else
			{
				return new DocRefNMFC(nMFCArticle, factoryToWrap);
			}
		}

		RefNMFC NMFCArticle
		{
			get { return (RefNMFC)WrappedObject; }
		}

		public override string ToString()
		{
			return Code;
		}

		public ZString Class
		{
			get { return NMFCArticle.FN_Class; }
		}

		public ZString Code
		{
			get { return NMFCArticle.FN_Code; }
		}

		public ZString ItemNo
		{
			get { return NMFCArticle.FN_ItemNo; }
		}

		public ZString Description
		{
			get { return NMFCArticle.FN_Description; }
		}

		public ZBool IsActive
		{
			get { return NMFCArticle.FN_IsActive; }
		}
	}
}
