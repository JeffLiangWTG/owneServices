using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Common.Collections;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Decompiled from System.ComponentModel so we can use it. ExtenderProvider stuff probably won't
	/// work considering the methods that make it functional are internal; not visible from .net when
	/// .net casts PropertyDescriptor to System.Windows.Forms.ReflectPropertyDescriptor.
	/// </summary>

	public class ReflectPropertyDescriptor : PropertyDescriptor
	{
		#region Ripped From ReflectTypeDescriptionProvider

		static readonly LRUCache<MemberInfo, Attribute[]> attributeCache = new LRUCache<MemberInfo, Attribute[]>();

		static Attribute[] ReflectGetAttributes(MemberInfo member)
		{
			Argument.NotNull(member, nameof(member)); // Suggested By ReviewBot 
			Attribute[] result = attributeCache[member];
			if (result == null)
			{
				object[] objs = member.GetCustomAttributes(typeof(Attribute), false);
				result = new Attribute[objs.Length];
				objs.CopyTo(result, 0);
				attributeCache.Add(member, result);
			}
			return result;
		}

		#endregion

		public ReflectPropertyDescriptor(Type componentClass, PropertyDescriptor oldReflectPropertyDescriptor, Attribute[] attributes)
			: base(oldReflectPropertyDescriptor, attributes)
		{
			Argument.NotNull(oldReflectPropertyDescriptor, nameof(oldReflectPropertyDescriptor)); // Suggested By ReviewBot 
			Argument.NotNull(componentClass, nameof(componentClass));
			State = new BitVector32();
			ComponentClass = componentClass;
			Type = oldReflectPropertyDescriptor.PropertyType;

			ReflectPropertyDescriptor descriptor1 = oldReflectPropertyDescriptor as ReflectPropertyDescriptor;
			if (descriptor1 != null)
			{
				if (descriptor1.ComponentType == componentClass)
				{
					PropInfo = descriptor1.PropInfo;
					getMethod = descriptor1.getMethod;
					setMethod = descriptor1.setMethod;
					ShouldSerializeMethod = descriptor1.ShouldSerializeMethod;
					ResetMethod = descriptor1.ResetMethod;
					defaultValue = descriptor1.defaultValue;
					ambientValue = descriptor1.ambientValue;
					State = descriptor1.State;
				}
				if (attributes != null)
				{
					Attribute[] attributeArray1 = attributes;
					for (int num1 = 0; num1 < attributeArray1.Length; num1++)
					{
						Attribute attribute1 = attributeArray1[num1];
						DefaultValueAttribute attribute2 = attribute1 as DefaultValueAttribute;
						if (attribute2 != null)
						{
							defaultValue = attribute2.Value;
							State[ReflectPropertyDescriptor.BitDefaultValueQueried] = true;
						}
						else
						{
							AmbientValueAttribute attribute3 = attribute1 as AmbientValueAttribute;
							if (attribute3 != null)
							{
								ambientValue = attribute3.Value;
								State[ReflectPropertyDescriptor.BitAmbientValueQueried] = true;
							}
						}
					}
				}
			}
		}

		public ReflectPropertyDescriptor(Type componentClass, string name, Type type, Attribute[] attributes)
			: base(name, attributes)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(componentClass, nameof(componentClass));
			State = new BitVector32();
			Type = type;
			ComponentClass = componentClass;
		}

		public ReflectPropertyDescriptor(Type componentClass, string name, Type type, PropertyInfo propInfo, MethodInfo getMethod, MethodInfo setMethod, Attribute[] attrs)
			: this(componentClass, name, type, attrs)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(componentClass, nameof(componentClass));
			PropInfo = propInfo;
			this.getMethod = getMethod;
			this.setMethod = setMethod;
			if ((getMethod != null && propInfo != null) && setMethod == null)
			{
				State[ReflectPropertyDescriptor.BitGetQueried | ReflectPropertyDescriptor.BitSetOnDemand] = true;
			}
			else
			{
				State[ReflectPropertyDescriptor.BitGetQueried | ReflectPropertyDescriptor.BitSetQueried] = true;
			}
		}

		public ReflectPropertyDescriptor(Type componentClass, string name, Type type, Type receiverType, MethodInfo getMethod, MethodInfo setMethod, Attribute[] attrs)
			: this(componentClass, name, type, attrs)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(componentClass, nameof(componentClass));
			Argument.NotNull(getMethod, nameof(getMethod));
			Argument.NotNull(setMethod, nameof(setMethod));
			ReceiverType = receiverType;
			this.getMethod = getMethod;
			this.setMethod = setMethod;
			State[ReflectPropertyDescriptor.BitGetQueried | ReflectPropertyDescriptor.BitSetQueried] = true;
		}

		public override void AddValueChanged(object component, EventHandler handler)
		{
			Argument.NotNull(component, "component");
			Argument.NotNull(handler, "handler");
			var descriptor1 = ChangedEventValue;
			if (descriptor1 != null && descriptor1.EventType != null && descriptor1.EventType.IsInstanceOfType(handler))
			{
				descriptor1.AddEventHandler(component, handler);
			}
			else
			{
				if (base.GetValueChangedHandler(component) == null)
				{
					var descriptor2 = IPropChangedEventValue;
					if (descriptor2 != null)
					{
						descriptor2.AddEventHandler(component, new PropertyChangedEventHandler(OnINotifyPropertyChanged));
					}
				}
				base.AddValueChanged(component, handler);
			}
		}

		public override bool CanResetValue(object component)
		{
			if (!IsExtender && !IsReadOnly)
			{
				if (DefaultValue != ReflectPropertyDescriptor.NoValue)
				{
					return !object.Equals(GetValue(component), DefaultValue);
				}
				if (ResetMethodValue != null)
				{
					var storedShouldSerializeMethodValue = ShouldSerializeMethodValue;
					if (storedShouldSerializeMethodValue != null)
					{
						component = GetInvocationTarget(ComponentClass, component);
						try
						{
							var result = storedShouldSerializeMethodValue.Invoke(component, null);
							return result != null && (bool)result;
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
						}
					}
					return true;
				}
				if (AmbientValue != NoValue)
				{
					return ShouldSerializeValue(component);
				}
			}
			return false;
		}

		internal bool ExtenderCanResetValue(IExtenderProvider provider, object component)
		{
			if (DefaultValue != NoValue)
			{
				return !object.Equals(ExtenderGetValue(provider, component), defaultValue);
			}
			if (ResetMethodValue == null)
			{
				return false;
			}
			MethodInfo info2 = ShouldSerializeMethodValue;
			if (info2 != null)
			{
				try
				{
					provider = (IExtenderProvider)GetInvocationTarget(ComponentClass, provider);
					var result = info2.Invoke(provider, new object[] { component });
					return result != null && (bool)result;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			return true;
		}

		internal Type ExtenderGetReceiverType()
		{
			return ReceiverType;
		}

		internal Type ExtenderGetType(IExtenderProvider provider)
		{
			return PropertyType;
		}

		internal object ExtenderGetValue(IExtenderProvider provider, object component)
		{
			if (provider != null)
			{
				provider = (IExtenderProvider)GetInvocationTarget(ComponentClass, provider);
				return GetMethodValue.Invoke(provider, new object[] { component });
			}
			return null;
		}

		internal void ExtenderResetValue(IExtenderProvider provider, object component, PropertyDescriptor notifyDesc)
		{
			if (DefaultValue != NoValue)
			{
				ExtenderSetValue(provider, component, DefaultValue, notifyDesc);
			}
			else if (AmbientValue != NoValue)
			{
				ExtenderSetValue(provider, component, AmbientValue, notifyDesc);
			}
			else if (ResetMethodValue != null)
			{
				var site = MemberDescriptor.GetSite(component);
				IComponentChangeService service = null;
				object obj1 = null;
				if (site != null)
				{
					service = (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
				}
				if (service != null)
				{
					obj1 = ExtenderGetValue(provider, component);
					try
					{
						service.OnComponentChanging(component, notifyDesc);
					}
					catch (CheckoutException exception1)
					{
						if (exception1 != CheckoutException.Canceled)
						{
							throw;
						}
						return;
					}
				}
				provider = (IExtenderProvider)GetInvocationTarget(ComponentClass, provider);
				if (ResetMethodValue != null)
				{
					ResetMethodValue.Invoke(provider, new object[] { component });
					if (service != null)
					{
						object obj2 = ExtenderGetValue(provider, component);
						service.OnComponentChanged(component, notifyDesc, obj1, obj2);
					}
				}
			}
		}

		internal void ExtenderSetValue(IExtenderProvider provider, object component, object value, PropertyDescriptor notifyDesc)
		{
			if (provider != null)
			{
				var site = MemberDescriptor.GetSite(component);
				IComponentChangeService service = null;
				object obj = null;
				if (site != null)
				{
					service = (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
				}
				if (service != null)
				{
					obj = ExtenderGetValue(provider, component);
					try
					{
						service.OnComponentChanging(component, notifyDesc);
					}
					catch (CheckoutException exception1)
					{
						if (exception1 != CheckoutException.Canceled)
						{
							throw;
						}
						return;
					}
				}
				provider = (IExtenderProvider)GetInvocationTarget(ComponentClass, provider);
				if (SetMethodValue != null)
				{
					SetMethodValue.Invoke(provider, new object[] { component, value });
					if (service != null)
					{
						service.OnComponentChanged(component, notifyDesc, obj, value);
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FillAttributes(IList attributeList)
		{
			if (attributeList != null)
			{
				var attributesOfComponent = TypeDescriptor.GetAttributes(PropertyType);

				foreach (var attribute in attributesOfComponent)
				{
					attributeList.Add(attribute);
				}
				var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
				var componentClassType = ComponentClass;
				int index1 = 0;
				while (componentClassType != null && componentClassType != typeof(object))
				{
					index1++;
					componentClassType = componentClassType.BaseType;
				}
				if (index1 > 0)
				{
					componentClassType = ComponentClass;
					Attribute[][] attributeJaggedArray1 = new Attribute[index1][];
					while (componentClassType != null && componentClassType != typeof(object))
					{
						MemberInfo infoOfType1 = null;
						if (IsExtender)
						{
							infoOfType1 = componentClassType.GetMethod("Get" + Name, flags);
						}
						else
						{
							infoOfType1 = componentClassType.GetProperty(Name, flags, null, PropertyType, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
						}
						if (infoOfType1 != null)
						{
							index1--;
							if (index1 >= 0 && attributeJaggedArray1.GetLength(0) > index1)
							{
								attributeJaggedArray1[index1] = ReflectGetAttributes(infoOfType1);
							}
						}
						componentClassType = componentClassType.BaseType;
					}
					Attribute[][] attributeJaggedArray2 = attributeJaggedArray1;
					for (int index2 = 0; index2 < attributeJaggedArray2.Length; index2++)
					{
						Attribute[] attributeArray1 = attributeJaggedArray2[index2];
						if (attributeArray1 != null)
						{
							Attribute[] attributeArray4 = attributeArray1;
							for (int index3 = 0; index3 < attributeArray4.Length; index3++)
							{
								Attribute attribute2 = attributeArray4[index3];
								AttributeProviderAttribute attribute3 = attribute2 as AttributeProviderAttribute;
								if (attribute3 != null)
								{
									Type typeOfAttribute3 = Type.GetType(attribute3.TypeName);
									if (typeOfAttribute3 != null)
									{
										Attribute[] attributeArray2 = null;
										if (!string.IsNullOrEmpty(attribute3.PropertyName))
										{
											var infoOfType2Array = typeOfAttribute3.GetMember(attribute3.PropertyName);
											var infoOfType2 = infoOfType2Array.Length > 0 ? infoOfType2Array[0] : null;
											if (infoOfType2 != null)
											{
												attributeArray2 = ReflectGetAttributes(infoOfType2);
											}
										}
										else
										{
											attributeArray2 = ReflectGetAttributes(typeOfAttribute3);
										}
										if (attributeArray2 != null)
										{
											Attribute[] attributeArray5 = attributeArray2;
											for (int index4 = 0; index4 < attributeArray5.Length; index4++)
											{
												Attribute attribute4 = attributeArray5[index4];
												attributeList.Add(attribute4);
											}
										}
									}
								}
							}
						}
					}
					Attribute[][] attributeJaggedArray3 = attributeJaggedArray1;
					for (int index5 = 0; index5 < attributeJaggedArray3.Length; index5++)
					{
						Attribute[] attributeArray3 = attributeJaggedArray3[index5];
						if (attributeArray3 != null)
						{
							Attribute[] attributeArray6 = attributeArray3;
							for (int index6 = 0; index6 < attributeArray6.Length; index6++)
							{
								Attribute attribute5 = attributeArray6[index6];
								attributeList.Add(attribute5);
							}
						}
					}
				}
				base.FillAttributes(attributeList);
				if (SetMethodValue == null)
				{
					attributeList.Add(ReadOnlyAttribute.Yes);
				}
			}
		}

		public override object GetValue(object component)
		{
			if (!IsExtender && component != null)
			{
				component = GetInvocationTarget(ComponentClass, component);
				try
				{
					return GetMethodValue.Invoke(component, null);
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					string text1 = null;
					var iComponent = component as IComponent;
					if (iComponent != null)
					{
						var site = iComponent.Site;
						if ((site != null) && (site.Name != null))
						{
							text1 = site.Name;
						}
					}
					if (text1 == null)
					{
						text1 = component.GetType().FullName;
					}
					if (exception is TargetInvocationException)
					{
						exception = exception.InnerException;
					}
					var text2 = exception.Message ?? exception.GetType().Name;
					throw new TargetInvocationException("ErrorPropertyAccessorException" + "|" + Name + "|" + text1 + "|" + text2, exception);
				}
			}
			return null;
		}

		internal void OnINotifyPropertyChanged(object component, PropertyChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(e.PropertyName) || (string.Compare(e.PropertyName, Name, true, CultureInfo.InvariantCulture) == 0))
			{
				OnValueChanged(component, e);
			}
		}

		protected override void OnValueChanged(object component, EventArgs e)
		{
			if (State[BitChangedQueried] && (RealChangedEvent == null))
			{
				base.OnValueChanged(component, e);
			}
		}

		public override void RemoveValueChanged(object component, EventHandler handler)
		{
			var descriptor1 = ChangedEventValue;
			if (descriptor1 != null)
			{
				descriptor1.RemoveEventHandler(component, handler);
			}
			else
			{
				base.RemoveValueChanged(component, handler);
				if (base.GetValueChangedHandler(component) == null)
				{
					var descriptor2 = IPropChangedEventValue;
					if (descriptor2 != null)
					{
						descriptor2.RemoveEventHandler(component, new PropertyChangedEventHandler(OnINotifyPropertyChanged));
					}
				}
			}
		}

		public override void ResetValue(object component)
		{
			var obj1 = GetInvocationTarget(ComponentClass, component);
			if (DefaultValue != NoValue)
			{
				SetValue(component, DefaultValue);
			}
			else if (AmbientValue != NoValue)
			{
				SetValue(component, AmbientValue);
			}
			else if (ResetMethodValue != null)
			{
				var site = MemberDescriptor.GetSite(component);
				IComponentChangeService service = null;
				object obj2 = null;
				if (site != null)
				{
					service = (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
				}
				if (service != null)
				{
					obj2 = GetMethodValue.Invoke(obj1, null);
					try
					{
						service.OnComponentChanging(component, this);
					}
					catch (CheckoutException exception1)
					{
						if (exception1 != CheckoutException.Canceled)
						{
							throw;
						}
						return;
					}
				}

				ResetMethodValue.Invoke(obj1, null);
				if (service != null)
				{
					object obj3 = GetMethodValue.Invoke(obj1, null);
					service.OnComponentChanged(component, this, obj2, obj3);
				}
			}
		}

		public override void SetValue(object component, object value)
		{
			if (component != null)
			{
				var site = MemberDescriptor.GetSite(component);
				IComponentChangeService service = null;
				object obj1 = null;
				var obj2 = GetInvocationTarget(ComponentClass, component);
				if (!IsReadOnly)
				{
					if (site != null)
					{
						service = (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
					}
					if (service != null)
					{
						obj1 = GetMethodValue.Invoke(obj2, null);
						try
						{
							service.OnComponentChanging(component, this);
						}
						catch (CheckoutException exception)
						{
							if (exception != CheckoutException.Canceled)
							{
								throw;
							}
							return;
						}
					}
					try
					{
						if (SetMethodValue != null)
						{
							SetMethodValue.Invoke(obj2, new object[] { value });
						}
						OnValueChanged(obj2, EventArgs.Empty);
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						value = obj1;
						if ((exception is TargetInvocationException) && (exception.InnerException != null))
						{
							throw exception.InnerException;
						}
						throw;
					}
					finally
					{
						if (service != null)
						{
							service.OnComponentChanged(component, this, obj1, value);
						}
					}
				}
			}
		}

		public override bool ShouldSerializeValue(object component)
		{
			component = GetInvocationTarget(ComponentClass, component);
			if (IsReadOnly)
			{
				if (ShouldSerializeMethodValue != null)
				{
					try
					{
						var result = ShouldSerializeMethodValue.Invoke(component, null);
						return result != null && (bool)result;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				}
				return Attributes != null && Attributes.Contains(DesignerSerializationVisibilityAttribute.Content);
			}
			if (DefaultValue != ReflectPropertyDescriptor.NoValue)
			{
				return !object.Equals(DefaultValue, GetValue(component));
			}
			if (ShouldSerializeMethodValue != null)
			{
				try
				{
					var result = ShouldSerializeMethodValue.Invoke(component, null);
					return result != null && (bool)result;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
			return true;
		}

		object AmbientValue
		{
			get
			{
				if (!State[BitAmbientValueQueried] && Attributes != null)
				{
					State[BitAmbientValueQueried] = true;
					var attribute = Attributes[typeof(AmbientValueAttribute)];
					if (attribute != null)
					{
						ambientValue = ((AmbientValueAttribute)attribute).Value;
					}
					else
					{
						ambientValue = NoValue;
					}
				}
				return ambientValue;
			}
		}

		EventDescriptor ChangedEventValue
		{
			get
			{
				if (!State[BitChangedQueried])
				{
					State[BitChangedQueried] = true;
					RealChangedEvent = TypeDescriptor.GetEvents(ComponentType)[string.Format(CultureInfo.InvariantCulture, "{0}Changed", new object[] { Name })];
				}
				return RealChangedEvent;
			}
		}

		public override Type ComponentType
		{
			get
			{
				return ComponentClass;
			}
		}

		object DefaultValue
		{
			get
			{
				if (!State[BitDefaultValueQueried] && Attributes != null)
				{
					State[BitDefaultValueQueried] = true;
					Attribute attribute1 = Attributes[typeof(DefaultValueAttribute)];
					if (attribute1 != null)
					{
						defaultValue = ((DefaultValueAttribute)attribute1).Value;
					}
					else
					{
						defaultValue = NoValue;
					}
				}
				return defaultValue;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Method name prefix")]
		protected MethodInfo GetMethodValue
		{
			get
			{
				if (!State[BitGetQueried])
				{
					State[BitGetQueried] = true;
					if (ReceiverType == null)
					{
						if (PropInfo == null)
						{
							BindingFlags flags1 = BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
							PropInfo = ComponentClass.GetProperty(Name, flags1, null, PropertyType, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
						}
						if (PropInfo != null)
						{
							getMethod = PropInfo.GetGetMethod(true);
						}
						if (getMethod == null)
						{
							State[BitGetQueried] = false;
							throw new InvalidOperationException("ErrorMissingPropertyAccessors" + "|" + ComponentClass.FullName + "." + Name);
						}
					}
					else
					{
						getMethod = MemberDescriptor.FindMethod(ComponentClass, "Get" + Name, new Type[] { ReceiverType }, Type);
						if (getMethod == null)
						{
							State[BitGetQueried] = false;
							throw new ArgumentException("ErrorMissingPropertyAccessors" + "|" + Name);
						}
					}
				}
				return getMethod;
			}
		}

		EventDescriptor IPropChangedEventValue
		{
			get
			{
				if (!State[BitIPropChangedQueried])
				{
					State[BitIPropChangedQueried] = true;
					if (typeof(INotifyPropertyChanged).IsAssignableFrom(ComponentType))
					{
						RealIPropChangedEvent = TypeDescriptor.GetEvents(typeof(INotifyPropertyChanged))["PropertyChanged"];
					}
				}
				return RealIPropChangedEvent;
			}
			set
			{
				RealIPropChangedEvent = value;
				State[BitIPropChangedQueried] = true;
			}
		}

		bool IsExtender
		{
			get
			{
				return (ReceiverType != null);
			}
		}

		public override bool IsReadOnly
		{
			get
			{
				if (SetMethodValue != null && Attributes != null)
				{
					var readOnlyAttribute = (ReadOnlyAttribute)Attributes[typeof(ReadOnlyAttribute)];
					return readOnlyAttribute != null && readOnlyAttribute.IsReadOnly;
				}
				return true;
			}
		}

		public override Type PropertyType
		{
			get
			{
				return Type;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Method name prefix")]
		MethodInfo ResetMethodValue
		{
			get
			{
				if (!State[BitResetQueried])
				{
					Type[] typeArray1;
					State[BitResetQueried] = true;
					if (ReceiverType == null)
					{
						typeArray1 = ArgsNone;
					}
					else
					{
						typeArray1 = new Type[] { ReceiverType };
					}
					ResetMethod = MemberDescriptor.FindMethod(ComponentClass, "Reset" + Name, typeArray1, typeof(void), false);
				}
				return ResetMethod;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Method name prefix")]
		protected MethodInfo SetMethodValue
		{
			get
			{
				if (!State[BitSetQueried] && State[BitSetOnDemand])
				{
					State[BitSetQueried] = true;
					var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
					if (PropInfo != null)
					{
						string text1 = PropInfo.Name;
						if (setMethod == null)
						{
							for (Type type1 = ComponentType.BaseType; type1 != null && type1 != typeof(object); type1 = type1.BaseType)
							{
								PropertyInfo info1 = type1.GetProperty(text1, flags, null, PropertyType, Array.Empty<Type>(), null);
								if (info1 != null)
								{
									setMethod = info1.GetSetMethod();
									if (setMethod != null)
									{
										break;
									}
								}
							}
						}
					}
				}
				if (!State[BitSetQueried])
				{
					State[BitSetQueried] = true;
					if (ReceiverType == null)
					{
						if (PropInfo == null)
						{
							var flags = BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
							PropInfo = ComponentClass.GetProperty(Name, flags, null, PropertyType, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
						}
						if (PropInfo != null)
						{
							setMethod = PropInfo.GetSetMethod(true);
						}
					}
					else
					{
						setMethod = MemberDescriptor.FindMethod(ComponentClass, "Set" + Name, new Type[] { ReceiverType, Type }, typeof(void));
					}
				}
				return setMethod;
			}
		}

		MethodInfo ShouldSerializeMethodValue
		{
			get
			{
				if (!State[BitShouldSerializeQueried])
				{
					Type[] typeArray1;
					State[BitShouldSerializeQueried] = true;
					if (ReceiverType == null)
					{
						typeArray1 = ArgsNone;
					}
					else
					{
						typeArray1 = new Type[] { ReceiverType };
					}
					ShouldSerializeMethod = MemberDescriptor.FindMethod(ComponentClass, "ShouldSerialize" + Name, typeArray1, typeof(bool), false);
				}
				return ShouldSerializeMethod;
			}
		}

		public override bool SupportsChangeEvents
		{
			get
			{
				if (IPropChangedEventValue == null)
				{
					return (ChangedEventValue != null);
				}
				return true;
			}
		}

		object ambientValue;

		[ThreadSafe]
		static readonly Type[] ArgsNone = Array.Empty<Type>();
		// The BitVector32 fields need to be in the correct order to properly resolve the masks
		static readonly int BitDefaultValueQueried = BitVector32.CreateMask();
		static readonly int BitGetQueried = BitVector32.CreateMask(BitDefaultValueQueried);
		static readonly int BitSetQueried = BitVector32.CreateMask(BitGetQueried);
		static readonly int BitShouldSerializeQueried = BitVector32.CreateMask(BitSetQueried);
		static readonly int BitResetQueried = BitVector32.CreateMask(BitShouldSerializeQueried);
		static readonly int BitChangedQueried = BitVector32.CreateMask(BitResetQueried); 
		static readonly int BitIPropChangedQueried = BitVector32.CreateMask(BitChangedQueried);
		static readonly int BitReadOnlyChecked = BitVector32.CreateMask(BitIPropChangedQueried);
		static readonly int BitAmbientValueQueried = BitVector32.CreateMask(BitReadOnlyChecked);
		static readonly int BitSetOnDemand = BitVector32.CreateMask(BitAmbientValueQueried);
		readonly Type ComponentClass;
		object defaultValue;
		MethodInfo getMethod;
		static readonly object NoValue = new object();
		PropertyInfo PropInfo;
		EventDescriptor RealChangedEvent;
		EventDescriptor RealIPropChangedEvent;
		readonly Type ReceiverType;
		MethodInfo ResetMethod;
		MethodInfo setMethod;
		MethodInfo ShouldSerializeMethod;
		BitVector32 State;
		readonly Type Type;
	}
}
