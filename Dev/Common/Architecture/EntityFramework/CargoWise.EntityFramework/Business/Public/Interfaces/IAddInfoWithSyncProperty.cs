using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IAddInfoWithSyncProperty
	{
		ZPropertyInfo AddInfoProperty { get; }
		void EnableSynchronization();
		IZType GetAddInfoValue(IZType data, Type addInfoValueType);
	}
}
