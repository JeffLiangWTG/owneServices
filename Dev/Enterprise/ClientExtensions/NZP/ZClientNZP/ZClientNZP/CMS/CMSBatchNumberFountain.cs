using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.NZP.CMS
{
	public class CMSBatchNumberFountain : FileNameNumberFountain
	{
		protected CMSBatchNumberFountain()
		{
		}

		public static FileNameNumberFountain NewDelegate()
		{
			return new CMSBatchNumberFountain();
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
		}

		#region Implementation

		protected override ZString FountainName
		{
			get { return "CMS Batch Number"; }
		}

		protected override long MaxValue
		{
			get { return 9220000000000000000; }
		}

		protected override ZString GenerateFilename(IDbConnected connected, bool progressNumber)
		{
			throw new NotImplementedException("Generate Filename is not implemented");
		}

		#endregion
	}
}
