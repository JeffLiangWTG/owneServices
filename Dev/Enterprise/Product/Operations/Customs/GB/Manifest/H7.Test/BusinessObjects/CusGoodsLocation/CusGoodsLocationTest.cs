using System;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	public class CusGoodsLocationTest : EU.H7.Business.Testing.CusGoodsLocationTest
	{
		protected override Type LookupType => typeof(CusGoodsLocationLookups);
	}
}
