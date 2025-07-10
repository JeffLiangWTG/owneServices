using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class DomainValidationGroup
	{
		public void RegisterValidationType<BusinessObjectT, ValidationT>()
			where BusinessObjectT : BusinessObject
			where ValidationT : ZValidation
		{
			RegisterValidationType(typeof(BusinessObjectT), typeof(ValidationT));
		}

		public void RegisterValidationType(Type businessObjectType, Type validationType)
		{
			CheckIfValidationTypeCanBeRegistered(businessObjectType, validationType);

			BusinessObjectDomainValidation businessObjectDomainValidation = GetBusinessObjectDomainValidation(businessObjectType);
			businessObjectDomainValidation.RegisterValidationType(validationType);
		}

		public void UnregisterValidationType<BusinessObjectT, ValidationT>()
			where BusinessObjectT : BusinessObject
			where ValidationT : ZValidation
		{
			UnregisterValidationType(typeof(BusinessObjectT), typeof(ValidationT));
		}

		public void UnregisterValidationType(Type businessObjectType, Type validationType)
		{
			BusinessObjectDomainValidation businessObjectDomainValidation = GetBusinessObjectDomainValidation(businessObjectType);
			businessObjectDomainValidation.UnregisterValidationType(validationType);
			if (businessObjectDomainValidation.IsEmpty)
			{
				BusinessObjectDomainValidations.Remove(businessObjectType);
			}
		}

		public bool ContainsDomainValidation(Type businessObjectType)
		{
			return ContainsDomainValidation(businessObjectType, null);
		}

		public bool ContainsDomainValidation(Type businessObjectType, Type validationType)
		{
			bool result;

			if (businessObjectType != null)
			{
				BusinessObjectDomainValidation bizODomainValidation = GetBusinessObjectDomainValidation(businessObjectType);
				result = bizODomainValidation.ContainsValidationType(validationType);
			}
			else
			{
				result = false;
			}

			return result;
		}

		public ZValidation[] CreateDomainValidation(BusinessObject businessObject)
		{
			ZValidation[] result;

			if (businessObject != null)
			{
				BusinessObjectDomainValidation bizODomainValidation = GetBusinessObjectDomainValidation(businessObject.GetType());
				result = bizODomainValidation.CreateAdditionalValidation(businessObject);
			}
			else
			{
				result = Array.Empty<ZValidation>();
			}

			return result;
		}

		#region class BusinessObjectDomainValidation

		public class BusinessObjectDomainValidation
		{
			public BusinessObjectDomainValidation(DomainValidationGroup domainValidation, Type businessObjectType)
			{
				this.DomainValidation = domainValidation;
				this.BusinessObjectType = businessObjectType;
			}

			public void RegisterValidationType(Type validationType)
			{
				if (!ValidationTypes.Contains(validationType))
				{
					ValidationTypes.Add(validationType);
					fAllValidationTypes = null;
				}
			}

			public void UnregisterValidationType(Type validationType)
			{
				if (ValidationTypes.Contains(validationType))
				{
					ValidationTypes.Remove(validationType);
					fAllValidationTypes = null;
				}
			}

			public ZValidation[] CreateAdditionalValidation(BusinessObject businessObject)
			{
				return NewValidationArrayMethod(businessObject);
			}

			public bool IsEmpty
			{
				get { return AllValidationTypes.Length == 0; }
			}

			public bool ContainsValidationType(Type validationType)
			{
				return (validationType == null) ? AllValidationTypes.Length > 0 : ((IList)AllValidationTypes).Contains(validationType);
			}

			#region Implementation

			NewValidationArrayDelegate NewValidationArrayMethod
			{
				get
				{
					if (fCurrentAllValidationTypesForMethod != AllValidationTypes)
					{
						fNewValidationArrayMethod = CreateNewValidationArrayMethod();
						fCurrentAllValidationTypesForMethod = AllValidationTypes;
					}
					return fNewValidationArrayMethod;
				}
			}

			NewValidationArrayDelegate CreateNewValidationArrayMethod()
			{
				DynamicMethod dynamicMethod = new DynamicMethod(NewValidationArrayMethodName, typeof(ZValidation[]), new Type[] { typeof(BusinessObject) }, GetType());
				ILGenerator generator = dynamicMethod.GetILGenerator();

				// COMMENT: ZValidation[] ResultLocalVar = new ZValidation[AllValidationTypes.Length];
				LocalBuilder resultLocalVar = generator.DeclareLocal(typeof(ZValidation[]));
				generator.Emit(OpCodes.Ldc_I4, AllValidationTypes.Length);
				generator.Emit(OpCodes.Newarr, typeof(ZValidation));
				generator.Emit(OpCodes.Stloc, resultLocalVar);

				for (int i = 0; i < AllValidationTypes.Length; i++)
				{
					ConstructorInfo constructor = DomainValidation.GetValidationConstructor(BusinessObjectType, AllValidationTypes[i]);

					// COMMENT: ResultLocalVar[i] = new MyValidation(BizObj);
					generator.Emit(OpCodes.Ldloc, resultLocalVar);
					generator.Emit(OpCodes.Ldc_I4, i);
					generator.Emit(OpCodes.Ldarg_0);
					generator.Emit(OpCodes.Castclass, constructor.GetParameters()[0].ParameterType);
					generator.Emit(OpCodes.Newobj, constructor);
					generator.Emit(OpCodes.Stelem_Ref);
				}

				// COMMENT: return ResultLocalVar;
				generator.Emit(OpCodes.Ldloc, resultLocalVar);
				generator.Emit(OpCodes.Ret);

				return (NewValidationArrayDelegate)dynamicMethod.CreateDelegate(typeof(NewValidationArrayDelegate));
			}

			string NewValidationArrayMethodName
			{
				get
				{
					string result = "DomainValidation_CreateNewValidationArray";

#if DEBUG
					result = "DomainValidation_";
					result += BusinessObjectType.FullName;
					foreach (Type validationType in AllValidationTypes)
					{
						result += "_" + validationType.FullName;
					}
#endif

					return result;
				}
			}

			Type[] AllValidationTypes
			{
				get
				{
					Type[] currentBaseValidationTypes = (Base != null) ? Base.AllValidationTypes : null;
					if (fAllValidationTypes == null || fBaseAllValidationTypes != currentBaseValidationTypes)
					{
						List<Type> list = new List<Type>();
						fBaseAllValidationTypes = currentBaseValidationTypes;
						if (fBaseAllValidationTypes != null)
						{
							list.AddRange(fBaseAllValidationTypes);
						}
						list.AddRange(ValidationTypes);
						fAllValidationTypes = list.ToArray();
					}
					return fAllValidationTypes;
				}
			}

			BusinessObjectDomainValidation Base
			{
				get
				{
					BusinessObjectDomainValidation result = null;
					if (BusinessObjectType.BaseType != typeof(BusinessObject))
					{
						result = DomainValidation.GetBusinessObjectDomainValidation(BusinessObjectType.BaseType);
					}
					return result;
				}
			}

			delegate ZValidation[] NewValidationArrayDelegate(BusinessObject bizObj);

			public readonly Type BusinessObjectType;
			readonly DomainValidationGroup DomainValidation;

			readonly List<Type> ValidationTypes = new List<Type>();
			NewValidationArrayDelegate fNewValidationArrayMethod;
			Type[] fCurrentAllValidationTypesForMethod;
			Type[] fAllValidationTypes;
			Type[] fBaseAllValidationTypes;

			#endregion
		}

		#endregion

		#region Implementation

		BusinessObjectDomainValidation GetBusinessObjectDomainValidation(Type businessObjectType)
		{
			BusinessObjectDomainValidation result;
			if (!BusinessObjectDomainValidations.TryGetValue(businessObjectType, out result))
			{
				result = new BusinessObjectDomainValidation(this, businessObjectType);
				BusinessObjectDomainValidations.Add(businessObjectType, result);
			}
			return result;
		}

		void CheckIfValidationTypeCanBeRegistered(Type businessObjectType, Type validationType)
		{
			if (validationType.IsAbstract)
			{
				throw new InvalidOperationException("ValidationType is abstract");
			}

			if (businessObjectType == typeof(BusinessObject))
			{
				throw new InvalidOperationException("Cannot register domain validation to base BusinessObject class");
			}

			if (GetValidationConstructor(businessObjectType, validationType) == null)
			{
				throw new InvalidOperationException("Expected a constructor with a single parameter of type or subclass of type " + businessObjectType.FullName + " for validation type " + validationType.FullName);
			}
		}

		ConstructorInfo GetValidationConstructor(Type businessObjectType, Type validationType)
		{
			foreach (ConstructorInfo constructor in validationType.GetConstructors())
			{
				ParameterInfo[] parameterInfos = constructor.GetParameters();
				if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.IsAssignableFrom(businessObjectType))
				{
					return constructor;
				}
			}

			return null;
		}

		internal Dictionary<Type, BusinessObjectDomainValidation> BusinessObjectDomainValidations = new Dictionary<Type, BusinessObjectDomainValidation>();

		#endregion
	}

	// TODO: extract these three DummyBizOValidation classes to DomainValidationGroupTest once Business/Testing has been extracted

	#region class DummyBizOValidationForTest

	public class DummyBizOValidationForTest : AutoDummyBizoValidation
	{
		public DummyBizOValidationForTest(AutoDummyBizo parent)
			: base(parent)
		{
		}

		protected override void CheckZ0_AnotherDate()
		{
			base.CheckZ0_AnotherDate();
			this.Parent.Z0_AnotherDateInfo.AddError((NoResString)"FROM TEST1");
		}
	}

	#endregion

	#region class DummyBizOValidationForTest2

	internal class DummyBizOValidationForTest2 : AutoDummyBizoValidation
	{
		public DummyBizOValidationForTest2(AutoDummyBizo parent)
			: base(parent)
		{
		}

		protected override void CheckZ0_AnotherDate()
		{
			base.CheckZ0_AnotherDate();
			Parent.Z0_AnotherDateInfo.AddError((NoResString)"FROM TEST2");
		}
	}

	#endregion

	#region class DummyBizOValidationForTest3

	internal class DummyBizOValidationForTest3 : AutoDummyBizoValidation
	{
		public DummyBizOValidationForTest3(AutoDummyBizo parent)
			: base(parent)
		{
		}

		protected override void CheckZ0_AnotherDate()
		{
			base.CheckZ0_AnotherDate();
			Parent.Z0_AnotherDateInfo.AddError((NoResString)"FROM TEST3");
		}
	}

	#endregion
}
