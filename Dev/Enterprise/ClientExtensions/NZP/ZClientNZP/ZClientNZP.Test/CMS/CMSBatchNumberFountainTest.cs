using System;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.NZP.CMS.Testing
{
	public class CMSBatchNumberFountainTest : FileNameNumberFountainTestCase
	{
		[ExpectException(typeof(NotImplementedException))]
		public void TestFileName()
		{
			CMSBatchNumberFountain.GetNewFileName();
		}

#region Implementation
		protected override bool LengthOfFileIDIsAlwaysTheSame
		{
			get
			{
				return false;
			}
		}

		protected override int FileIDLength
		{
			get
			{
				return 0;
			}
		}

		protected override FileNameNumberFountain FileNameNumberFountainInstance
		{
			get
			{
				return CMSBatchNumberFountain.New();
			}
		}

		protected override long MaxValue
		{
			get
			{
				return 9220000000000000000;
			}
		}
#endregion
	}
}
