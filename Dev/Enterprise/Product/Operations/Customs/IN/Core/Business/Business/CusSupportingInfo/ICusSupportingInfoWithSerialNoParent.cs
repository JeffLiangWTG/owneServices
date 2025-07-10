using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IN.Business;

public interface ICusSupportingInfoWithSerialNoParent : ICusSupportingInfoTypeSupporter
{
	HugeSequenceNumberGenerator GetSequenceNumberGenerator(string type);
}

public static class CusSupportingInfoWithSerialNoParentExtensions
{
	public static IDisposable GetLineNumberSuspenders(this ICusSupportingInfoWithSerialNoParent parent)
	{
		return new DisposableList(parent.GetCusSupportingInfoTypes().Keys.Select(type => parent.GetSequenceNumberGenerator(type)).WhereNotNull().Select(x => x.GetLineNumberSuspender()));
	}
}
