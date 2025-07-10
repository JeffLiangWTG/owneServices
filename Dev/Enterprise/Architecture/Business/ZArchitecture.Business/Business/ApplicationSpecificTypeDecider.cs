using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class ApplicationSpecificTypeDecider : TypeDecider
	{
		#region struct ApplicationSpecificType

		/// <remarks>
		/// It should be a class, not a struct to keep lazy cache of type object.
		/// </remarks>
		[WTG.StaticAnalysis.Annotation.Immutable]
		public sealed class ApplicationSpecificType
		{
			public ApplicationSpecificType(ZString applicationCode, Func<Type> businessObjectTypeGetter)
			{
				ApplicationCode = applicationCode;
				businessObjectType = new Lazy<Type>(businessObjectTypeGetter);
			}

			public Type BusinessObjectType
			{
				get
				{
					return businessObjectType.Value;
				}
			}

			public readonly ZString ApplicationCode;
			readonly Lazy<Type> businessObjectType;
		}

		#endregion

		protected ApplicationSpecificTypeDecider()
		{
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return null;
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		public IEnumerable<ApplicationSpecificType> ApplicationSpecificTypes => applicationSpecificTypes ?? (applicationSpecificTypes = ApplicationSpecificTypesCore);

		protected abstract IEnumerable<ApplicationSpecificType> ApplicationSpecificTypesCore { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Used in the above ApplicationSpecificTypes, configured in subclasses")]
		IEnumerable<ApplicationSpecificType> applicationSpecificTypes;

		public Type GetTypeForApplicationCode(ZString applicationCode)
		{
			foreach (ApplicationSpecificType applicationSpecificType in ApplicationSpecificTypes)
			{
				if (applicationSpecificType.ApplicationCode == applicationCode)
				{
					return applicationSpecificType.BusinessObjectType;
				}
			}

			return null;
		}
	}
}
