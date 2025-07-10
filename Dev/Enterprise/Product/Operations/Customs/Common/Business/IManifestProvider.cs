using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Common
{
	public interface IManifestProvider : IBusiness
	{
		EDIMessageCollection Messages { get; }
		event EventHandler CustomsManifestVisibilityChanged;
	}
}
