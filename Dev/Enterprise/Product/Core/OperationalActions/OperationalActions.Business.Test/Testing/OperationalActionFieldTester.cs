using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using Enterprise.Services.OperationalActions.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public class OperationalActionFieldTester : IOperationalActionFieldTester
	{
		public OperationalActionFieldTester()
		{
			typesChecked = new HashSet<Type>();
		}

		readonly HashSet<Type> typesChecked;
		public bool IsFieldUnsupported(Type type, string propertyName)
		{
			// Sometimes we wish to ensure certain fields will NEVER be shown in OperationalActions (even if architecture is changed)
			// We cannot use "[ActionField(ReadOnly = true)]" for this if the field isn't currently supported
			// This test will alert us if the architecture changes and we should update usages to be readonly instead.
			var prop = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).First(p => p.Name == propertyName);
			return new OperationalActionFieldGenerator().CreateField(new PropertyInfo[] { prop }) == null;
		}

		public void TestFields<T>(bool testChildren) => TestFields(typeof(T), testChildren);
		public void TestFields(Type type, bool testChildren)
		{
			var errors = FindErrorFields(type, testChildren);
			if (errors.Any())
			{
				var invalidFieldsFoundMessage = string.Format("Operational action fields without field supporters were found{0}{0}", System.Environment.NewLine);
				throw new Exception(errors.Aggregate(invalidFieldsFoundMessage, (curr, next) => curr + next + System.Environment.NewLine));
			}
		}

		protected IEnumerable<string> FindErrorFields(Type type, bool testChildren = true)
		{
			var generator = new OperationalActionFieldGenerator();
			var properties =
				from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				group p by p.Name into actionProperties
				select actionProperties.FirstOrDefault();
			var errorFields = new List<string>();
			typesChecked.Add(type);
			ErrorReporter.Clear();
			UnitTestUserNotification.Instance.ClearMessages();
			foreach (var prop in properties)
			{
				switch (ReflectionHelper.Classify(prop))
				{
					case PropertyClassification.Updatable:
						generator.CreateField(new PropertyInfo[] { prop });
						if (!string.IsNullOrEmpty(ErrorReporter.LastMessageReported))
						{
							errorFields.Add(string.Format("{0}.{1}", type.Name, prop.Name));
							ErrorReporter.Clear();
						}

						break;
					case PropertyClassification.FollowSingle:
					case PropertyClassification.FollowCollection:
						if (testChildren)
						{
							Type childType = ActionFieldFollowAttribute.GetReturnType(prop);
							if (!typesChecked.Contains(childType))
							{
								errorFields.AddRange(FindErrorFields(childType, testChildren));
							}
						}

						break;
				}
			}

			return errorFields;
		}
	}
}
