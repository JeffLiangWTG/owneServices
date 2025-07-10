using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IUser
	{
		ZString Code { get; }
		ZString Name { get; }
		ZString Phone { get; }
		ZString Email { get; }
		ZString Fax { get; }
		ZBool IsDeveloper { get; }
		object Signature { get; }
		IReadOnlyCollection<ICertificate> Certificates { get; }
		IReadOnlyCollection<IUserGroup> Groups { get; }
	}
}
